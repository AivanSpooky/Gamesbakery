<template>
  <div :class="['review', className]">
    <p class="p">{{ text }}</p>
    <div class="layout">
      <ReviewUser :user="user" />
      <ReviewDate :date="date" />
      <ReviewRating :rating="rating" />
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import ReviewUser from './ReviewUser.vue';
  import ReviewDate from './ReviewDate.vue';
  import ReviewRating from './ReviewRating.vue';
  export default defineComponent({
    name: 'Review',
    components: { ReviewUser, ReviewDate, ReviewRating },
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Это текст отзыва. Может быть большой...' },
      user: { type: String, default: 'Anonymous' },
      date: { type: String, default: new Date().toLocaleDateString() },
      rating: { type: Number, default: 0 },
    },
  });
</script>
<style scoped>
  .review {
    background-color: var(--color-background);
    height: 210px;
    overflow: hidden;
    position: relative;
    width: 100%;
  }

  .p {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-small);
    left: 17px;
    letter-spacing: 0;
    line-height: normal;
    position: absolute;
    top: 105px;
    width: calc(100% - 34px);
  }

  .layout {
    align-items: center;
    display: flex;
    gap: 6px;
    left: 0;
    position: absolute;
    top: 0;
  }
</style>
