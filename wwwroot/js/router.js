const routes = [
    {
        path: '/',
        name: 'Home',
        component: httpVueLoader('../vue/about.vue'),
    },
    {
        path: '/admin',
        name: 'Admin',
        component: httpVueLoader('../vue/admin.vue'),
    },
    {
        path: '/play/:publicToken',
        name: 'Play',
        component: httpVueLoader('../vue/play.vue'),
    },
]

const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes: routes,
})

export default router
