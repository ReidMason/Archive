import Vue from 'vue';
import Vuex from 'vuex';

Vue.use(Vuex)

export const store = new Vuex.Store({
    state: {
        message: 'New message!'
    },
    mutations: {
        changeMessage(state, message) {
            state.message = message;
        }
    },
    getters: {
        message: state => state.message
    }
})