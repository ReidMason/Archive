import Vue from 'vue';
import Vuex from 'vuex';

Vue.use(Vuex)

export const store = new Vuex.Store({
    state: {
        endpoint: "https://localhost:44379",
        inventoryItems: null,
        itemToEdit: "blank"
    },
    mutations: {
        setInventoryItems: function (state, data) {
            state.inventoryItems = data;
        },
        setItemToEdit: function (state, item) {
            state.itemToEdit = item;
        }
    },
    getters: {
        inventoryItems: state => state.inventoryItems,
        endpoint: state => state.endpoint,
        itemToEdit: state => state.itemToEdit
    }
})