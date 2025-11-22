<template>
  <div class="create-review view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <BigTitle text="Отзыв к игре" class="max-w-1662 mx-auto" />
        <div class="input-layout max-w-1662 mx-auto">
          <InputExample inputLblText="Игра:" v-model="gameTitle" readonly />
          <InputExample inputLblText="Рейтинг (1-5):" v-model="rating" type="number" min="1" max="5" />
          <Description v-model="text" placeholder="Тут текст отзыва..." :title="'Текст отзыва'" :readonly="false" />
          <ActionsExample haveAccLblText="Отмена" @haveAccClick="goBack" regLblText="Оставить отзыв" @regClick="submitAndBack" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import InputExample from '@/components/ui/InputExample.vue';
  import Description from '@/components/ui/game/Description.vue';
  import ActionsExample from '@/components/ui/ActionsExample.vue';
  import { useReviewCreate } from '@/composables/useReview';

  const { gameTitle, rating, text, submitAndBack, goBack } = useReviewCreate();
</script>

<style scoped>
  .create-review {
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

  .input-layout {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
    max-width: 1546px;
  }
</style>
