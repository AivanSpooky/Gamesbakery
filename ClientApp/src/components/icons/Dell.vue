<template>
  <svg :width="size" :height="size" viewBox="0 0 72 72" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M18 18L54 54M54 18L18 54" stroke="var(--color-text-primary)" stroke-width="8" />
  </svg>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    props: { size: { type: Number, default: 72 } },
  });
</script>
