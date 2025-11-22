<template>
  <div :class="['order-row', className]">
    <TableGameLbl :text="orderId" />
    <TablePriceLbl :text="totalPrice + ' ₽'" />
    <TableSellerLbl :text="date" />
    <TableGameLbl :text="status" />
    <router-link :to="toDetails">
      <MoreAboutGameBtn @click="$emit('click')" />
    </router-link>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import TableGameLbl from '@/components/ui/table/TableGameLbl.vue';
  import TablePriceLbl from '@/components/ui/table/TablePriceLbl.vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';
  import MoreAboutGameBtn from '@/components/ui/game/MoreAboutGameBtn.vue';
  export default defineComponent({
    name: 'OrderRow',
    components: { TableGameLbl, TablePriceLbl, TableSellerLbl, MoreAboutGameBtn },
    props: {
      className: { type: String, default: '' },
      orderId: { type: String, required: true },
      totalPrice: { type: Number, required: true },
      date: { type: String, required: true },
      status: { type: String, required: true },
      toDetails: { type: String, required: true },
    },
    emits: ['click'],
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
