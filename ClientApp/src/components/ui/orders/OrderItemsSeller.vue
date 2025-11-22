<template>
  <div :class="['order-item-seller', className]">
    <TableGameLbl :text="gameText" />
    <TablePriceLbl :text="priceText + ' ₽'" />
    <TableSellerLbl :text="statusText" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import TableGameLbl from '@/components/ui/table/TableGameLbl.vue';
  import TablePriceLbl from '@/components/ui/table/TablePriceLbl.vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';

  export default defineComponent({
    name: 'OrderItemSeller',
    components: { TableGameLbl, TablePriceLbl, TableSellerLbl },
    props: {
      className: { type: String, default: '' },
      gameText: { type: String, required: true },
      priceText: { type: Number, required: true },
      statusText: { type: String, required: true },
    },
  });
</script>
<style scoped>
  .order-item-seller {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 1rem;
    width: 100%;
    overflow: hidden;
    justify-content: space-between;
  }
</style>
