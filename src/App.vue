<template>
  <div id="app" class="container">
    <div class="row">
      <div class="col-sm">
        <transition-group name="fade" tag="div">
          <InventoryItem v-for="item in $store.getters.inventoryItems" :item="item" :key="item.id" />
        </transition-group>
      </div>
      <div class="col-sm">
        <InventoryItemForm @form-submitted="addItem" :submitText="'Save'" :cancelText="'Clear'" />
      </div>
    </div>
    <InventoryItemEditModal />
  </div>
</template>

<script>
import { store } from "./store/store";
import axios from "axios";
import InventoryItem from "./components/InventoryItem";
import InventoryItemEditModal from "./components/InventoryItemEditModal";
import InventoryItemForm from "./components/InventoryItemForm";

export default {
  name: "App",
  store,
  components: {
    InventoryItem,
    InventoryItemEditModal,
    InventoryItemForm
  },
  created: function() {
    axios.get(`${store.getters.endpoint}/InventoryItems/`).then(response => {
      store.commit("setInventoryItems", response.data.data);
    });
  },
  methods: {
    addItem: function(itemData) {
      console.log(this.$store.getters.itemToEdit);
      axios
        .post(
          `${this.$store.getters.endpoint}/InventoryItems/AddInventoryItem`,
          itemData,
          {
            headers: {
              "Content-Type": "application/json"
            }
          }
        )
        .then(response =>
          this.$store.commit("setInventoryItems", response.data.data)
        );
    }
  }
};
</script>

<style>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s;
}

.fade-enter,
.fade-leave-to {
  opacity: 0;
}
</style>