import Vue from "vue";
import VueRouter from "vue-router";
import Home from "./components/views/Home";
import About from "./components/views/About";
import NotFound from "./components/views/NotFound"

Vue.use(VueRouter);

const router = new VueRouter({
    mode: 'history',
    routes: [
        {
            path: '/',
            component: Home
        },
        {
            path: '/about',
            component: About
        },
        {
            // Redirect to home on any invalid urls
            path: "*",
            component: NotFound
        }
    ]
})

export default router;