<template>
  <b-nav class="d-flex gap-4 p-2 align-items-center">
    <h2 class="title text-light">Navbar</h2>
    <div class="text-light toggle-button" @click="toggleSidebar">
      Toggle sidebar
    </div>
    <b-nav-item :to="{ path: '/' }">Home</b-nav-item>
    <b-nav-item :to="{ path: '/about' }">About</b-nav-item>
  </b-nav>
</template>

<script>
export default {
  name: "Navbar",
  props: {
    toggleSidebar: Function,
  },
};
</script>

<style scoped>
.toggle-button {
  cursor: pointer;
}

.nav {
  background-color: #343a40;
  width: 100%;
  height: 56px;
  position: fixed;
}
</style>