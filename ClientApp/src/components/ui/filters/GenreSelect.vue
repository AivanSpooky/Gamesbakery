<template>
  <div class="genre-select">
    <CategoryLbl class="rectangle-3" />
    <select class="rectangle-2" v-model="selectedGenre">
      <option>Стратегии</option>
      <option>Action</option>
      <option>RPG</option>
      <!-- Dynamic from API -->
    </select>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref } from 'vue';
  import CategoryLbl from './CategoryLbl.vue';

  export default defineComponent({
    name: 'GenreSelect',
    components: { CategoryLbl },
    setup() {
      const selectedGenre = ref('Стратегии');
      return { selectedGenre };
    },
  });
</script>

<style scoped>
  .genre-select {
    position: relative;
  }

  .rectangle-3 {
    position: absolute;
    left: 0;
    top: 0;
  }

  .rectangle-2 {
    background-color: var(--color-teal-accent);
    border: 6px solid var(--color-teal-border);
    border-radius: 40px;
    height: 90px;
    position: absolute;
    left: 232px;
    top: 0;
    width: 354px;
    color: #ffffff;
    font-family: "Rixel-Regular", Helvetica;
    font-size: 32px;
    text-align: center;
  }
</style>
