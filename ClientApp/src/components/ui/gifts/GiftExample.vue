<template>
  <div :class="['gift-example', className]">
    <div class="layout flex items-center gap-4">
      <TableSellerLbl :text="from" />
      <TableSellerLblWrapper :text="date" />
      <TablePriceLbl :text="status" />
      <MoreAboutGameBtn />
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';
  import TableSellerLblWrapper from '@/components/ui/table/TableSellerLblWrapper.vue'; // Assuming this exists or create if needed
  import TablePriceLbl from '@/components/ui/table/TablePriceLbl.vue';
  import MoreAboutGameBtn from '@/components/ui/game/MoreAboutGameBtn.vue';
  export default defineComponent({
    name: 'GiftExample',
    components: { TableSellerLbl, TableSellerLblWrapper, TablePriceLbl, MoreAboutGameBtn },
    props: {
      className: { type: String, default: '' },
      from: { type: String, default: 'User-12' },
      date: { type: String, default: '11/12/2024'},
      type: { type: String, default: '' }, // Not used in template, but from Figma
      status: { type: String, default: 'Подарен' },
    },
  });
</script>
<style scoped>
  .gift-example {
    background-color: var(--color-background);
    border: 12px solid var(--color-background-border);
    border-radius: var(--radius-main);
    overflow: hidden;
    position: relative;
    width: 100%;
  }

  .layout {
    align-items: center;
    display: flex;
    gap: 11px;
    height: 121px;
    justify-content: center;
    padding: 18px 30px;
    position: relative;
    width: 100%;
  }
</style>
