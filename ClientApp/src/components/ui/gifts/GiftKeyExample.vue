<template>
  <div :class="['gift-key-example', className]">
    <div class="layout items-center gap-4">
      <TableGameLbl text="Ключ от игры:" />
      <div class="key-game">
        <div class="text-wrapper">{{ gameTitle }}</div>
      </div>
      <TableDeleteBtn @click="$emit('remove')" />
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import TableGameLbl from '@/components/ui/table/TableGameLbl.vue';
  import TableDeleteBtn from '@/components/ui/table/TableDeleteBtn.vue';
  export default defineComponent({
    name: 'GiftKeyExample',
    components: { TableGameLbl, TableDeleteBtn },
    props: {
      className: { type: String, default: '' },
      gameTitle: { type: String, default: 'Game 1' },
    },
    emits: ['remove'],
  });
</script>

<style scoped>
  .gift-key-example {
    display: flex;
    position: relative;
    width: 100%;
  }

  .layout {
    align-items: center;
    display: flex;
    gap: 10px;
    height: 121px;
    justify-content: center;
    padding: 18px 30px;
    position: relative;
    width: 100%;
  }

  .key-game {
    background-color: var(--color-money-accent);
    border: 12px solid var(--color-yellow-border);
    border-radius: var(--radius-main);
    height: 85px;
    position: relative;
    flex: 1; /* Flexible width to prevent overlap */
    min-width: 200px; /* Minimum to avoid collapse */
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    white-space: nowrap;
  }
</style>
