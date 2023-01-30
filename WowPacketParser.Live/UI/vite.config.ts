import {defineConfig} from 'vite'
import vue from '@vitejs/plugin-vue'
import VueTypeImports from "vite-plugin-vue-type-imports";

export default defineConfig({
    plugins: [vue(), VueTypeImports()],
    resolve: {
        alias: {
            vue: "vue/dist/vue.esm-bundler.js"
        }
    }
})
