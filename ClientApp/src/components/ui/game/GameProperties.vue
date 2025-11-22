<template>
  <div :class="['game-properties', className]">
    <div class="content">
      <GameLbl :text="game.title" />
      <Description :text="game.description" />
      <div class="filters">
        <PublisherLbl class="publisher-lbl-instance" />
        <Publisher class="publisher-instance" :text="game.publisher" />
        <PriceLbl class="price-lbl-instance" />
        <Price class="price-instance" :text="game.price" />
        <CategoryLbl class="category-lbl-instance" />
        <Category class="category-instance" :value="game.category" readonly />
        <DateLbl class="date-lbl-instance" />
        <Date class="date-instance" :text="game.releaseDate" />
        <ReviewsBtn class="reviews-btn-instance" @click="goToReviews" />
      </div>
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import { useRouter } from 'vue-router';
  import GameLbl from './GameLbl.vue';
  import Description from './Description.vue';
  import PublisherLbl from './PublisherLbl.vue';
  import Publisher from './Publisher.vue';
  import PriceLbl from './PriceLbl.vue';
  import Price from './Price.vue';
  import CategoryLbl from './CategoryLbl.vue';
  import Category from './Category.vue';
  import DateLbl from './DateLbl.vue';
  import Date from './Date.vue';
  import ReviewsBtn from './ReviewsBtn.vue';
  export default defineComponent({
    name: 'GameProperties',
    components: { GameLbl, Description, PublisherLbl, Publisher, PriceLbl, Price, CategoryLbl, Category, DateLbl, Date, ReviewsBtn },
    props: {
      className: { type: String, default: '' },
      game: { type: Object, required: true },
    },
    setup(props) {
      const router = useRouter();
      const goToReviews = () => router.push(`/game/details/${props.game.id}/reviews`);
      return { goToReviews };
    },
  });
</script>
<style scoped>
  .game-properties {
    background-color: var(--color-background);
    display: flex;
    height: 350px;
    position: relative;
    width: 100%;
    max-width: 1662px;
    margin: 0 auto;
    overflow: hidden;
  }

  .content {
    background-color: var(--color-background);
    border: var(--border-notification) solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 321px;
    margin-left: 11px;
    margin-top: 21px;
    overflow: hidden;
    position: relative;
    width: calc(100% - 22px);
  }

  .filters {
    background-color: var(--color-background-accent);
    border: var(--border-notification) solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 210px;
    left: 31.9%;
    overflow: hidden;
    position: absolute;
    top: -1px;
    width: 68.17%;
  }

  .publisher-lbl-instance {
    left: 490px;
    position: absolute;
    top: 11px;
  }

  .publisher-instance {
    left: 714px;
    position: absolute;
    top: 11px;
  }

  .price-lbl-instance {
    left: 24px;
    position: absolute;
    top: 106px;
  }

  .price-instance {
    left: 216px;
    position: absolute;
    top: 106px;
  }

  .category-lbl-instance {
    left: 24px;
    position: absolute;
    top: 11px;
  }

  .category-instance {
    left: 180px;
    position: absolute;
    top: 11px;
  }

  .date-lbl-instance {
    left: 453px;
    position: absolute;
    top: 106px;
  }

  .date-instance {
    left: 714px;
    position: absolute;
    top: 106px;
  }

  .reviews-btn-instance {
    left: 951px;
    position: absolute;
    top: 11px;
  }
</style>
