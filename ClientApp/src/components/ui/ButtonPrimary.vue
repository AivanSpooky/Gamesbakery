<template>
  <button class="btn-primary neon-button" :disabled="disabled">
    <slot />
  </button>
</template>

<script setup lang="ts">
  defineProps<{
    disabled?: boolean;
  }>();
</script>
