<template>
  <div class="big-title">
    <div class="rectangle flex items-center justify-center">
      <span class="text-wrapper">
        {{ text }}
      </span>
    </div>
  </div>
</template>
<script setup lang="ts">
  defineProps<{
    text: string;
  }>();
</script>
<style scoped>
  .big-title {
    position: relative;
    width: 100%;
    max-width: 1546px;
    height: 119px;
  }

  .rectangle {
    background-color: var(--color-background-accent);
    border: 12px solid var(--color-border-1);
    border-radius: var(--radius-main); /* 40px */
    position: absolute;
    top: -1px;
    left: 0;
    width: 100%;
    height: 120px;
  }

  .text-wrapper {
    color: var(--color-text-primary); /* #ffffff */
    font-family: var(--font-additional), Helvetica; /* Roboto with fallback */
    font-size: var(--font-size-large); /* 64px */
    font-weight: 400;
    letter-spacing: 0;
    line-height: normal;
    text-align: center;
    white-space: nowrap;
  }
</style>
