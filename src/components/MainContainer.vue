<template>
  <div class="container">
    <h1>message</h1>
    <InventoryItem />
    <button v-on:click="changeMessage">Change message</button>
  </div>
</template>

<script>
import InventoryItem from "./InventoryItem.vue";

export default {
  name: "MainContainer",
  components: {
    InventoryItem
  },
  methods: {
    changeMessage: function() {
      this.$store.commit("changeMessage", "This message has been changed!");
    }
  }
};
</script>