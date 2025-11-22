<template>
  <div :class="['order-row', className]">
    <TableGameLbl :text="gameText" />
    <TablePriceLbl :text="priceText" />
    <TableSellerLbl :text="sellerText" />
    <TableDeleteBtn @click="$emit('delete')" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import TableGameLbl from '@/components/ui/table/TableGameLbl.vue';
  import TablePriceLbl from '@/components/ui/table/TablePriceLbl.vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';
  import TableDeleteBtn from '@/components/ui/table/TableDeleteBtn.vue';
  export default defineComponent({
    name: 'CartItemRow',
    components: { TableGameLbl, TablePriceLbl, TableSellerLbl, TableDeleteBtn },
    props: {
      className: { type: String, default: '' },
      gameText: { type: String, required: true },
      priceText: { type: String, required: true },
      sellerText: { type: String, required: true },
    },
    emits: ['delete'],
  });
</script>
<style scoped>
  .order-row {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    gap: 2rem;
    padding: 1rem;
    width: 100%;
    overflow: hidden;
    justify-content: space-between;
  }
</style>
