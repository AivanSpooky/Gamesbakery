<template>
  <svg :width="size" :height="size" viewBox="0 0 72 72" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle cx="36" cy="36" r="30" stroke="var(--color-text-primary)" stroke-width="6" />
    <path d="M20 36L30 46L52 24" stroke="var(--color-text-primary)" stroke-width="6" />
  </svg>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    props: { size: { type: Number, default: 72 } },
  });
</script>
