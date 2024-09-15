<template>
  <div class="container">
    <h1>message</h1>

    <button v-on:click="changeMessage">Change message</button>
  </div>
</template>

<script>
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