<template>
  <div :class="['review-rating', className]">
    <div class="text-wrapper">{{ rating }}/5</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'ReviewRating',
    props: {
      className: { type: String, default: '' },
      rating: { type: Number, default: 5 },
    },
  });
</script>

<style scoped>
  .review-rating {
    background-color: var(--color-success); /* Assuming accessinner is success */
    border: var(--border-notification) solid var(--color-border-2); /* accessborder */
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 71px;
    width: 124px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
  }
</style>
