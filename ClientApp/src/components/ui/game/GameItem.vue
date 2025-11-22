<template>
  <div :class="['game-item', className, { 'unavailable': !available }]">
    <GamePageBtn class="design-component-instance-node" :gamePageBtnText="gamePageBtnText" />
    <GamePriceLbl class="design-component-instance-node" :gamePriceLblText="gamePriceLblText" />
    <CanBuyBtn v-if="available" class="design-component-instance-node" />
    <CannotBuyBtn v-else class="design-component-instance-node" />
    <MoreAboutGameBtn class="design-component-instance-node" @click="goToGame" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import { useRouter } from 'vue-router';
  import GamePageBtn from './GamePageBtn.vue';
  import GamePriceLbl from './GamePriceLbl.vue';
  import CanBuyBtn from './CanBuyBtn.vue';
  import CannotBuyBtn from './CannotBuyBtn.vue';
  import MoreAboutGameBtn from './MoreAboutGameBtn.vue';
  export default defineComponent({
    name: 'GameItem',
    components: { GamePageBtn, GamePriceLbl, CanBuyBtn, CannotBuyBtn, MoreAboutGameBtn },
    props: {
      className: { type: String, default: '' },
      available: { type: Boolean, default: true },
      gamePageBtnText: { type: String, default: 'Game 1' },
      gamePriceLblText: { type: String, default: '249 ₽' },
      id: { type: String, required: true },
    },
    setup(props) {
      const router = useRouter();
      const goToGame = () => router.push(`/game/details/${props.id}`);  // Changed 'games' to 'game'
      return { goToGame };
    },
  });
</script>
<style scoped>
  .game-item {
    align-items: center;
    background-color: var(--color-background);
    display: flex;
    gap: 17px;
    justify-content: flex-start;
    padding: 10px 29px;
    position: relative;
    border-radius: var(--radius-main);
    width: 100%;
  }

  .design-component-instance-node {
    position: relative !important;
  }
</style>
