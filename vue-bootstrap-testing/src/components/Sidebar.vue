<template>
  <div class="text-white" id="sidebar">
    <b-sidebar
      body-class="custom-sidebar"
      no-close-on-route-change
      :visible="visible"
      bg-variant="dark"
      no-header
      width="225px"
    >
      <div class="p-4">
        <div v-for="dropdown in sidebarDropdowns" :key="dropdown.id">
          <SidebarDropdown
            class="dropdown"
            :toggleVisible="toggleVisible"
            :dropdownData="dropdown"
          />
        </div>
      </div>
    </b-sidebar>
  </div>
</template>

<script>
import SidebarDropdown from "./SidebarDropdown";
import { BIconLayoutTextWindowReverse, BIconBook } from "bootstrap-vue";

export default {
  name: "Sidebar",
  components: {
    SidebarDropdown,
  },
  props: {
    visible: Boolean,
  },
  data: function () {
    return {
      sidebarDropdowns: [
        {
          id: 1,
          heading: "Interface",
          icon: BIconLayoutTextWindowReverse,
          visible: false,
          items: [
            {
              text: "To about page",
              link: "/about",
            },
            {
              text: "Light sidenav",
            },
          ],
        },
        {
          id: 2,
          heading: "Pages",
          icon: BIconBook,
          visible: false,
          items: [
            {
              text: "Authentication",
            },
            {
              text: "Error",
            },
          ],
        },
      ],
      expandedDropdownId: null,
    };
  },
  methods: {
    toggleVisible(dropdownId) {
      this.sidebarDropdowns.forEach((dropdown) => {
        if (dropdown.visible && dropdown.id === dropdownId)
          dropdown.visible = false;
        else dropdown.visible = dropdown.id === dropdownId;
      });
    },
  },
};
</script>

<style scoped>
.dropdown {
  margin-bottom: 20px;
}
</style>

<style>
.b-sidebar {
  margin-top: 56px !important;
}
</style>