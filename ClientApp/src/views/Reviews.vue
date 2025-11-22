<template>
  <div class="reviews view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <div class="border-2 max-w-1662 mx-auto" />
        <div class="header-row max-w-1662 mx-auto">
          <GameLbl :text="gameTitle" />
          <ReviewsLbl text="Отзывы" />
          <ReviewsLbl text="Оставить отзыв" isButton @click="goToCreateReview" />
        </div>
        <div class="reviews-list max-w-1662 mx-auto">
          <Review v-for="review in displayedReviews" :key="review.id" :text="review.text" :user="review.username" :date="formatDate(review.creationDate)" :rating="review.rating" />
        </div>
        <div class="flex justify-center mt-32 max-w-1662 mx-auto">
          <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { useRouter } from 'vue-router';
  import GameLbl from '@/components/ui/game/GameLbl.vue';
  import ReviewsLbl from '@/components/ui/reviews/ReviewsLbl.vue';
  import Review from '@/components/ui/reviews/Review.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useReviewsList } from '@/composables/useReview';
  import { usePagination } from '@/composables/usePagination';
  import type { Review as ReviewType } from '@/types/review';

  const router = useRouter();
  const { gameTitle, reviews, formatDate, gameId } = useReviewsList();

  const { page, totalPages, displayedItems: displayedReviews } = usePagination<ReviewType>(reviews);

  const goToCreateReview = () => router.push(`/review/create?gameId=${gameId}`);
</script>

<style scoped>
  .reviews {
    position: relative;
    height: 100vh;
    overflow: auto;
    background-color: var(--color-background);
    display: flex;
    justify-content: center;
    align-items: flex-start;
  }

  .bg {
    aspect-ratio: 1.5;
    height: 1404px;
    left: -79px;
    position: absolute;
    top: -157px;
    width: 2105px;
    z-index: -1;
  }

  .img {
    height: 100%;
    left: 3.75%;
    object-fit: cover;
    position: absolute;
    top: 11.18%;
    width: 100%;
  }

  .welcome-screen {
    background-color: #001199;
    padding: 40px;
    border-radius: 20px;
    border: 4px solid #00d8ff;
    width: 100vw;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    box-sizing: border-box;
  }

  .border-2 {
    background-color: #001199;
    border: 6px solid #00d8ff;
    border-radius: 40px;
    height: 120px;
    width: 100%;
  }

  .header-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    gap: 20px;
    position: absolute;
    top: 44px;
    left: 0;
    padding: 0 20px;
  }

  .reviews-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
    margin-top: 40px;
  }
</style>
