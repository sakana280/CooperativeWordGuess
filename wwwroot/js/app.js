import router from './router.js'

const vueapp = {
    template: `<router-view />`,
}

Vue.createApp(vueapp)
    .use(router)
    .mount('#app')

console.log('Sorry cheaters you wont find the answer client side 😋')