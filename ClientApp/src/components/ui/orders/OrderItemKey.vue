<template>
  <div :class="['order-item-key', className]">
    <TableGameLbl :text="gameText" />
    <TableSellerLbl :text="sellerText" />
    <TablePriceLbl :text="keyText" bg-color="var(--color-money-accent)" border-color="var(--color-yellow-border)" />
    <TableDeleteBtn :text="'Показать'" bg-color="var(--color-btn-delete)" border-color="var(--color-border-3)" @click="$emit('show-key')" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import TableGameLbl from '@/components/ui/table/TableGameLbl.vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';
  import TablePriceLbl from '@/components/ui/table/TablePriceLbl.vue';
  import TableDeleteBtn from '@/components/ui/table/TableDeleteBtn.vue';
  export default defineComponent({
    name: 'OrderItemKey',
    components: { TableGameLbl, TableSellerLbl, TablePriceLbl, TableDeleteBtn },
    props: {
      className: { type: String, default: '' },
      gameText: { type: String, required: true },
      sellerText: { type: String, required: true },
      keyText: { type: String, required: true },
    },
    emits: ['show-key'],
  });
</script>
<style scoped>
  .order-item-key {
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
