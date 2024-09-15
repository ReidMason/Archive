<template>
  <div class="text-muted">
    <div
      class="heading justify-content-between align-items-center d-flex"
      @click="toggleVisible(dropdownData.id)"
    >
      <div class="heading-text d-flex align-items-center gap-2">
        <component :is="dropdownData.icon"></component>
        <p>{{ dropdownData.heading }}</p>
      </div>

      <BIconChevronRight v-if="!dropdownData.visible" />
      <BIconChevronDown v-else />
    </div>

    <div class="collapse-wrapper">
      <b-collapse :visible="dropdownData.visible">
        <div
          class="collapse-element"
          v-for="(item, i) in dropdownData.items"
          :key="i"
        >
          <router-link v-if="item.link" :to="item.link">{{
            item.text
          }}</router-link>
          <p v-else>{{ item.text }}</p>
        </div>
      </b-collapse>
    </div>
  </div>
</template>

<script>
import { BIconChevronRight, BIconChevronDown } from "bootstrap-vue";

export default {
  name: "SidebarDropdown",
  components: {
    BIconChevronRight,
    BIconChevronDown,
  },
  props: {
    dropdownData: Object,
    toggleVisible: Function,
  },
};
</script>

<style scoped>
p {
  margin: 0;
}

.collapse-wrapper {
  margin-left: 25px;
}

.collapse-element,
.heading {
  margin-top: 5px;
  cursor: pointer;
}

.collapse-element:hover,
.heading:hover .heading-text {
  color: white;
}
</style>