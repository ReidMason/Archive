<template>
  <form v-on:submit="formSubmitted" v-if="form">
    <div
      v-for="[field, fieldType] in Object.entries($store.getters.inventoryItemFields || {})"
      :key="field"
      class="form-group"
    >
      <label>{{ field }}</label>
      <input
        :type="fieldType"
        class="form-control"
        v-model="form[field]"
        autocomplete="off"
        required
      />
    </div>
    <button class="btn btn-primary" @click="formSubmitted" data-dismiss="modal">{{ submitText }}</button>
    <button
      type="button"
      class="btn btn-secondary ml-5"
      @click="formCancelled"
      v-if="cancelText !== undefined"
      data-dismiss="modal"
    >{{ cancelText }}</button>
  </form>
</template>

<script>
export default {
  name: "InventoryItemForm",
  props: ["item", "submitText", "cancelText"],
  computed: {
    form: function() {
      return this.item || this.newItem;
    }
  },
  data: function() {
    return {
      newItem: {}
    };
  },
  methods: {
    formSubmitted: async function(e) {
      e.preventDefault();
      this.form["quantity"] = parseInt(this.form["quantity"], 10);
      this.$emit("form-submitted", this.form);
      this.newItem = {};
    },
    formCancelled: function() {
      this.newItem = {};
      this.$emit("form-cancelled");
    }
  }
};
</script>