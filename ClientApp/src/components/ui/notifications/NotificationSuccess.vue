<template>
  <div :class="['notification-success', className]">
    <p class="text-wrapper">{{ message }}</p>
    <button @click="$emit('close')" class="close-btn">X</button>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'NotificationSuccess',
    props: {
      className: { type: String, default: '' },
      message: { type: String, required: true },
    },
    emits: ['close'],
  });
</script>
<style scoped>
  .notification-success {
    background-color: var(--color-success);
    border: var(--border-notification) solid var(--color-border-2);
    border-radius: var(--radius-button);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    width: 612px;
    overflow: hidden;
    position: relative;
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }

  .close-btn {
    position: absolute;
    top: 10px;
    right: 10px;
    background: transparent;
    border: none;
    color: var(--color-text-secondary);
    font-size: 16px;
    cursor: pointer;
  }
</style>
