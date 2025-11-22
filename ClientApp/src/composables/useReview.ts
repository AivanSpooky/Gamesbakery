import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ReviewService } from '@/services/ReviewService';
import { GameService } from '@/services/GameService';
import { useNotifications } from '@/composables/useNotifications';
import type { Review } from '@/types';

export function useReviewCreate() {
  const route = useRoute();
  const router = useRouter();
  const gameId = route.query.gameId as string || '1';
  const gameTitle = ref('');
  const rating = ref('5');
  const text = ref('');
  const { showSuccess, showError } = useNotifications();
  const submitAndBack = async () => {
    const ratingNum = parseInt(rating.value, 10);
    if (isNaN(ratingNum) || ratingNum < 1 || ratingNum > 5) {
      showError('Рейтинг должен быть числом от 1 до 5');
      return;
    }
    await ReviewService.create(gameId, ratingNum, text.value);
    showSuccess('Отзыв оставлен');
    router.back();
  };
  const goBack = () => router.back();
  onMounted(async () => {
    const game = await GameService.getById(gameId);
    gameTitle.value = game.title;
  });
  return { gameTitle, rating, text, submitAndBack, goBack };
}

export function useReviewsList() {
  const route = useRoute();
  const gameId = route.params.id as string;
  const gameTitle = ref('');
  const reviews = ref<Review[]>([]);
  const { showError } = useNotifications();
  const loadGame = async () => {
    try {
      const game = await GameService.getById(gameId);
      gameTitle.value = game.title || 'Unknown Game';
    } catch (err) {
      showError('Ошибка загрузки игры');
      gameTitle.value = 'Unknown Game';
    }
  };
  const loadReviews = async () => {
    try {
      const res = await ReviewService.getByGameId(gameId); // High limit for "all"
      reviews.value = (res || []).map((r: any) => ({
        id: r.id,
        text: r.text,
        username: r.username || 'Anonymous',
        creationDate: r.creationDate,
        rating: r.rating,
      }));
    } catch (err: any) {
      showError('Ошибка загрузки отзывов');
      reviews.value = [];
    }
  };
  const formatDate = (dateStr: string) => new Date(dateStr).toLocaleDateString() || 'Invalid date';
  onMounted(async () => {
    await loadGame();
    await loadReviews();
  });
  return { gameTitle, reviews, formatDate, gameId, loadReviews };
}
