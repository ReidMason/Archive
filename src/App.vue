<template>
  <div id="app" class="container">
    <div class="row mt-3">
      <div class="col-sm">
        <h2 class="mb-3">Inventory items</h2>
        <transition-group name="fade" tag="div">
          <InventoryItem v-for="item in $store.getters.inventoryItems" :item="item" :key="item.id" />
        </transition-group>
        <Spinner v-if="loadingInventoryItems" />
        <div
          :class="'alert alert-' + ($store.getters.inventoryItems === null ? 'danger' : 'primary')"
          role="alert"
          v-if="!loadingInventoryItems && (!$store.getters.inventoryItems || $store.getters.inventoryItems.length == 0)"
        >{{$store.getters.inventoryItems === null ? 'Unable to connect to server' : 'No results found'}}</div>
      </div>
      <div class="col-sm mt-5">
        <InventoryItemForm
          @form-submitted="addNewItem"
          :submitText="'Save'"
          :cancelText="'Clear'"
          v-if="$store.getters.inventoryItemFields"
        />
      </div>
    </div>
    <InventoryItemEditModal />
  </div>
</template>

<script>
import { store } from "./store/store";
import InventoryItem from "./components/InventoryItem";
import InventoryItemEditModal from "./components/InventoryItemEditModal";
import InventoryItemForm from "./components/InventoryItemForm";
import Spinner from "./components/Spinner";

export default {
  name: "App",
  store,
  components: {
    InventoryItem,
    InventoryItemEditModal,
    InventoryItemForm,
    Spinner
  },
  data: function() {
    return {
      loadingInventoryItems: true
    };
  },
  created: function() {
    store
      .dispatch("getInventoryItems")
      .then(response => {
        store.commit("setInventoryItems", response.data.data);
      })
      .finally(() => (this.loadingInventoryItems = false));

    store.dispatch("getInventoryItemFields").then(response => {
      store.commit("setInventoryItemFields", response.data.data);
    });
  },
  methods: {
    addNewItem: function(newItem) {
      store
        .dispatch("addInventoryItem", newItem)
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