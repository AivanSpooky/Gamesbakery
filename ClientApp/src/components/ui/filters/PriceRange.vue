<template>
  <div class="price-range">
    <PriceLbl class="rectangle-4" />
    <Price class="rectangle-6" />
    <DateLbl class="rectangle-5" text="до" />
    <Date class="rectangle-7" />
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import PriceLbl from './PriceLbl.vue';
import Price from './Price.vue';
import DateLbl from './DateLbl.vue';
import Date from './Date.vue';

export default defineComponent({
  name: 'PriceRange',
  components: { PriceLbl, Price, DateLbl, Date },
});
</script>

<style scoped>
.price-range {
  position: relative;
}
.rectangle-4 {
  position: absolute;
  left: 0;
  top: 0;
}
.rectangle-6 {
  position: absolute;
  left: 192px;
  top: 0;
}
.rectangle-5 {
  position: absolute;
  left: 320px;
  top: 0;
}
.rectangle-7 {
  position: absolute;
  left: 448px;
  top: 0;
}
</style>
