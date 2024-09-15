<template>
  <div class="d-flex flex-column align-items-stretch">
    <Navbar :toggleSidebar="toggleSidebar" />
    <div class="d-flex wrapper">
      <Sidebar :visible="this.sidebarVisible" />
      <div
        class="content-wrapper"
        :class="{ 'content-offset': sidebarVisible }"
      >
        <router-view />
      </div>
    </div>
  </div>
</template>

<script>
import Sidebar from "./components/Sidebar";
import Navbar from "./components/Navbar";

export default {
  name: "App",
  components: {
    Sidebar,
    Navbar,
  },
  data: function () {
    return {
      sidebarVisible: true,
    };
  },
  methods: {
    toggleSidebar() {
      console.log("Toggling sidebar");
      this.sidebarVisible = !this.sidebarVisible;
    },
  },
};
</script>

<style scoped>
.wrapper {
  /* Offset to match the navbar */
  margin-top: 56px;
}

.content-offset {
  /* Offset to match the sidebar */
  margin-left: 225px;
}

.content-wrapper {
  transition: margin 300ms ease-in-out;
}
</style>
