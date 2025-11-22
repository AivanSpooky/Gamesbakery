<template>
  <div class="log-out-btn cursor-pointer" @click="$emit('click')">
    <div class="labels">
      <div class="text-wrapper">Выйти</div>
      <SignOutSquare class="sign-out-square" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import SignOutSquare from '@/components/icons/SignOutSquare.vue';
  defineEmits<{
    click: [];
  }>();
</script>

<style scoped>
  .log-out-btn {
    background-color: var(--color-background-accent);
    border: 12px solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 230px;
    overflow: hidden;
  }

  .labels {
    display: flex;
    align-items: center;
    gap: 11px;
    padding: 0 1px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
  }
</style>
