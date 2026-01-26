import { useTranslation } from "react-i18next";
import { Check } from "lucide-react";
import { cn } from "@/lib/utils";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

interface Step {
  number: number;
  title: string;
  description: string;
}

interface StepIndicatorProps {
  steps: Step[];
  currentStep: number;
  className?: string;
}

export default function StepIndicator({
  steps,
  currentStep,
  className,
}: StepIndicatorProps) {
  const { t } = useTranslation();

  return (
    <div className={cn("space-y-4", className)}>
      {/* Step Counter Text */}
      <p className="text-xs font-medium text-slate-500 text-center">
        {t(i18nKeyContainer.appointment.stepOf, {
          current: currentStep,
          total: steps.length,
        })}
      </p>

      {/* Step Circles */}
      <div className="flex items-center justify-center gap-3">
        {steps.map((step, index) => {
          const isCompleted = currentStep > step.number;
          const isActive = currentStep === step.number;

          return (
            <div key={step.number} className="flex items-center">
              {/* Step Circle */}
              <div
                className={cn(
                  "flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all duration-200",
                  isCompleted && "border-primary bg-primary text-white",
                  isActive && "border-primary bg-primary/10 text-primary",
                  !isCompleted &&
                    !isActive &&
                    "border-slate-200 bg-slate-50 text-slate-400"
                )}
              >
                {isCompleted ? (
                  <Check className="h-5 w-5" />
                ) : (
                  <span className="text-sm font-semibold">{step.number}</span>
                )}
              </div>

              {/* Connector Line (except for last step) */}
              {index < steps.length - 1 && (
                <div
                  className={cn(
                    "mx-2 h-0.5 w-12 transition-all duration-200",
                    currentStep > step.number ? "bg-primary" : "bg-slate-200"
                  )}
                />
              )}
            </div>
          );
        })}
      </div>

      {/* Current Step Title & Description */}
      <div className="text-center">
        <h3 className="text-base font-semibold text-slate-900">
          {steps[currentStep - 1]?.title}
        </h3>
        <p className="mt-1 text-sm text-slate-500">
          {steps[currentStep - 1]?.description}
        </p>
      </div>
    </div>
  );
}
