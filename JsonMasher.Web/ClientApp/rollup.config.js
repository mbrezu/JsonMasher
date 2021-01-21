import svelte from 'rollup-plugin-svelte';
import commonjs from '@rollup/plugin-commonjs';
import resolve from '@rollup/plugin-node-resolve';
import livereload from 'rollup-plugin-livereload';
import { terser } from 'rollup-plugin-terser';
import sveltePreprocess from 'svelte-preprocess';
import typescript from '@rollup/plugin-typescript';
import css from 'rollup-plugin-css-only';
import md5 from 'md5';
import rimraf from "rimraf";
import fs from "fs";

const production = !process.env.ROLLUP_WATCH;

function serve() {
    let server;

    function toExit() {
        if (server) server.kill(0);
    }

    return {
        writeBundle() {
            if (server) return;
            server = require('child_process').spawn('npm', ['run', 'start', '--', '--dev'], {
                stdio: ['ignore', 'inherit', 'inherit'],
                shell: true
            });

            process.on('SIGTERM', toExit);
            process.on('exit', toExit);
        }
    };
}

// Quick and dirty hashing for static files.
function postProcess() {
    let fileNames = {};
    function hashName(file, extractor)
    {
        const key = file.fileName;
        file.fileName = "static/" + md5(extractor(file)) + "_" + file.fileName;
        fileNames[key] = file.fileName;
    }
    function hashAndCopy(fileName)
    {
        var content = fs.readFileSync("public/" + fileName);
        var md5Hash = md5(content);
        fileNames[fileName] = "static/" + md5Hash + "_" + fileName;
        fs.copyFileSync("public/" + fileName, "public/build/static/" + md5Hash + "_" + fileName);
    }
    return {
        name: "postProcess",
        buildStart() {
            rimraf.sync("public/build");
            fs.mkdirSync("public/build");
            fs.mkdirSync("public/build/static");
        }, 
        generateBundle(_, bundle) {
            hashName(bundle["bundle.js"], f => f.code);
            hashName(bundle["bundle.css"], f => f.source);
            bundle["bundle.js"].map = null;
        },
        writeBundle() {
            hashAndCopy("favicon.png");
            hashAndCopy("global.css");
            var index = fs.readFileSync("public/index.html").toString();
            index = index.replace("/favicon.png", fileNames["favicon.png"]);
            index = index.replace("/global.css", fileNames["global.css"]);
            index = index.replace("/build/bundle.css", fileNames["bundle.css"]);
            index = index.replace("/build/bundle.js", fileNames["bundle.js"]);
            fs.writeFileSync("public/build/index.html", index);
            console.log(fileNames);
        }
    };
}

export default {
    input: 'src/main.ts',
    output: {
        sourcemap: true,
        format: 'iife',
        name: 'app',
        file: 'public/build/bundle.js'
    },
    plugins: [
        svelte({
            preprocess: sveltePreprocess(),
            compilerOptions: {
                // enable run-time checks when not in production
                dev: !production
            }
        }),
        // we'll extract any component CSS out into
        // a separate file - better for performance
        css({ output: 'bundle.css' }),

        // If you have external dependencies installed from
        // npm, you'll most likely need these plugins. In
        // some cases you'll need additional configuration -
        // consult the documentation for details:
        // https://github.com/rollup/plugins/tree/master/packages/commonjs
        resolve({
            browser: true,
            dedupe: ['svelte']
        }),
        commonjs(),
        typescript({
            sourceMap: !production,
            inlineSources: !production
        }),

        // In dev mode, call `npm run start` once
        // the bundle has been generated
        !production && serve(),

        // Watch the `public` directory and refresh the
        // browser on changes when not in production
        !production && livereload('public'),

        // If we're building for production (npm run build
        // instead of npm run dev), minify
        production && terser(),
        production && postProcess()
    ],
    watch: {
        clearScreen: false
    }
};
