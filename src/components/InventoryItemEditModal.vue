<template>
  <div
    class="modal fade"
    id="editItemModal"
    ref="editItemModal"
    data-backdrop="static"
    tabindex="-1"
    role="dialog"
    aria-labelledby="staticBackdropLabel"
    aria-hidden="true"
  >
    <div class="modal-dialog" role="document">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="staticBackdropLabel">Edit Item</h5>
          <button type="button" class="close" data-dismiss="modal" aria-label="Close">
            <span aria-hidden="true">&times;</span>
          </button>
        </div>
        <div class="modal-body">
          <InventoryItemForm
            :item="$store.getters.itemToEdit"
            :submit-text="'Save'"
            :cancel-text="'Cancel'"
            @form-submitted="editItem"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import InventoryItemForm from "./InventoryItemForm.vue";
import axios from "axios";

export default {
  name: "InventoryItemEditModal",
  components: {
    InventoryItemForm
  },
  methods: {
    editItem: function(itemData) {
      axios
        .put(
          `${this.$store.getters.endpoint}/InventoryItems/EditInventoryItem/`,
          itemData
        )
        .then(response =>
          this.$store.commit("setInventoryItems", response.data.data)
        );
    }
  }
};
</script>