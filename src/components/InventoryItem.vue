<template>
  <div class="card w-75 m-2">
    <div class="card-body">
      <h5 class="card-title">{{ item.name }}</h5>
      <p class="card-text">
        Location: {{ item.location }}
        <br />
        Quantity: {{ item.quantity }}
        <br />
        Description: {{ item.description }}
      </p>
      <a
        class="btn btn-info"
        data-toggle="modal"
        data-target="#editItemModal"
        @click="setItemToEdit"
      >Edit</a>
      <a @click="deleteItem" style="float: right;" class="btn btn-danger">Delete</a>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "InventoryItem",
  props: {
    item: Object
  },
  methods: {
    deleteItem: function() {
      axios
        .delete(
          `${this.$store.getters.endpoint}/InventoryItems/DeleteInventoryItem/${this.item.id}`
        )
        .then(response =>
          this.$store.commit("setInventoryItems", response.data.data)
        );
    },
    setItemToEdit: function() {
      this.$store.commit("setItemToEdit", Object.assign({}, this.item));
      console.log(this.$store.getters.itemToEdit);
    }
  }
};
</script>