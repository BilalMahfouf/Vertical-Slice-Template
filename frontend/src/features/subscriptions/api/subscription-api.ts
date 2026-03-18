import api from '@/lib/api/api';

export interface CheckoutResponse {
  checkoutUrl: string;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  slug: string;
  amount: number;
  currency: string;
  billingInterval: string;
  intervalCount: number;
  trialDays: number;
  isActive: boolean;
  createdOnUtc: string;
}

interface CreateCheckoutRequest {
  planId: string;
}

const subscriptionApi = {
  getSubscriptionPlans: async (): Promise<SubscriptionPlan[]> => {
    const response = await api.get<SubscriptionPlan[]>('/subscription-plans');
    if (response.status !== 200) {
      throw new Error('Failed to fetch subscription plans');
    }
    return response.data;
  },

  createCheckout: async (planId?: string): Promise<CheckoutResponse> => {
    const payload: CreateCheckoutRequest | Record<string, never> =
      planId ? { planId } : {};

    const response = await api.post<CheckoutResponse>('/payments/checkout', payload);
    if (response.status !== 200 && response.status !== 201) {
      throw new Error('Failed to create checkout');
    }
    return response.data;
  },
};

export default subscriptionApi;
