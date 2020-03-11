<template>
  <div id="app">
    <MainContainer />
  </div>
</template>

<script>
import { store } from "./store/store";
import MainContainer from "./components/MainContainer.vue";

export default {
  name: "App",
  store,
  components: {
    MainContainer
  }
};
</script>