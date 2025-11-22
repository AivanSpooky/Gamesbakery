<template>
  <div class="actions-link flex items-center gap-2">
    <span class="have-acc">{{ text }}</span>
    <router-link :to="to" class="reg-link">{{ linkText }}</router-link>
  </div>
</template>

<script setup lang="ts">
  defineProps<{
    text: string;
    linkText: string;
    to: string;
  }>();
</script>

<style scoped>
  .actions-link {
    justify-content: center;
    color: var(--color-text-primary);
    font-size: var(--font-size-medium);
  }

  .reg-link {
    color: var(--color-additional-accent);
    text-decoration: underline;
    cursor: pointer;
  }
</style>
