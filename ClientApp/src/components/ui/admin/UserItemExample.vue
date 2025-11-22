<template>
  <div :class="['user-item-example', className, 'flex items-center gap-2']">
    <TableSellerLbl :text="name" />
    <TablePriceLbl :text="detail" />
    <TableDeleteBtn v-if="action1Text" :text="action1Text" @click="$emit('action1')" />
    <TableDeleteBtn v-if="action2Text" :text="action2Text" @click="$emit('action2')" />
    <MoreAboutGameBtn @click="$emit('more')" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';
  import TablePriceLbl from '@/components/ui/table/TablePriceLbl.vue';
  import TableDeleteBtn from '@/components/ui/table/TableDeleteBtn.vue';
  import MoreAboutGameBtn from '@/components/ui/game/MoreAboutGameBtn.vue';
  export default defineComponent({
    name: 'UserItemExample',
    components: { TableSellerLbl, TablePriceLbl, TableDeleteBtn, MoreAboutGameBtn },
    props: {
      className: { type: String, default: '' },
      name: { type: String, required: true },
      detail: { type: String, required: true },
      action1Text: { type: String, default: '' },
      action2Text: { type: String, default: '' },
    },
    emits: ['action1', 'action2', 'more'],
  });
</script>
<style scoped>
  .user-item-example {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 1rem;
    width: 100%;
    overflow: hidden;
  }
</style>
