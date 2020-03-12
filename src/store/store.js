import axios from "axios";
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
    },
    actions: {
        getInventoryItems(context) {
            return axios.get(`${context.getters.endpoint}/InventoryItems/`);
        },
        getInventoryItemFields(context) {
            return axios.get(`${context.getters.endpoint}/InventoryItems/GetInventoryFields`);
        },
        addInventoryItem(context, newItem) {
            return axios
                .post(
                    `${context.getters.endpoint}/InventoryItems/AddInventoryItem`,
                    newItem,
                    {
                        headers: {
                            "Content-Type": "application/json"
                        }
                    }
                )
        },
        editInventoryItem(context, itemData) {
            return axios
                .put(
                    `${context.getters.endpoint}/InventoryItems/EditInventoryItem/`,
                    itemData
                )
        },
        deleteInventoryItem(context, itemId) {
            return axios.delete(`${context.getters.endpoint}/InventoryItems/DeleteInventoryItem/${itemId}`);
        }
    }
})