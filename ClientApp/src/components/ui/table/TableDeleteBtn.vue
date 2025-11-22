<template>
  <div :class="['table-delete-btn', className]" :style="{ backgroundColor: bgColor, borderColor: borderColor }" @click="$emit('click')">
    <div class="text-wrapper" :style="{ color: textColor }">{{ text || 'Удалить' }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'TableDeleteBtn',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '' },
      bgColor: { type: String, default: 'var(--color-btn-delete)' },
      borderColor: { type: String, default: 'var(--color-border-3)' },
      textColor: { type: String, default: 'var(--color-text-primary)' },
    },
    emits: ['click'],
  });
</script>
<style scoped>
  .table-delete-btn {
    border: var(--border-interactive) solid;
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    overflow: hidden;
    position: relative;
    flex: 1;
    cursor: pointer;
  }

  .text-wrapper {
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    height: 38px;
    letter-spacing: 0;
    line-height: normal;
    text-align: center;
    white-space: nowrap;
    width: 126px;
  }
</style>
