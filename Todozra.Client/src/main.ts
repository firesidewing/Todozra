import { createApp } from 'vue'
import './style.css'
// @ts-ignore
import App from './App.vue'
import { VueQueryPlugin } from '@tanstack/vue-query'

createApp(App)
    .use(VueQueryPlugin)
    .mount('#app')
