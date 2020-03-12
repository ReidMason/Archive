import Vue from 'vue';
import Vuex from 'vuex';

Vue.use(Vuex)

export const store = new Vuex.Store({
    state: {
        endpoint: "https://localhost:5001",
        inventoryItemFields: null,
        inventoryItems: null,
        itemToEdit: null
    },
    mutations: {
        setInventoryItems: function (state, data) {
            state.inventoryItems = data;
        },
        setInventoryItemFields: function (state, data) {
            state.inventoryItemFields = data;
        },
        setItemToEdit: function (state, item) {
            state.itemToEdit = item;
        }
    },
    getters: {
        inventoryItems: state => state.inventoryItems,
        inventoryItemFields: state => state.inventoryItemFields,
        endpoint: state => state.endpoint,
        itemToEdit: state => state.itemToEdit
    }
})