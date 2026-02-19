import { useTranslation } from "react-i18next";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import {
  Building2,
  MapPin,
  AlertTriangle,
  Stethoscope,
  FileText,
  Pill,
  User,
  PawPrint,
  DollarSign,
  CreditCard,
} from "lucide-react";
import { type Client } from "@/features/clients/client-api";
import { type ClientAnimal } from "@/features/animals/animal-api";
import { VisitType, PaymentStatus } from "../visit-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { Input } from "@/components/ui/input";

export interface VisitFormData {
  visitType: number;
  symptoms: string;
  diagnosis: string;
  treatment: string;
  notes: string;
  paymentAmount: number;
  paymentStatus: number;
}

interface VisitDetailsStepProps {
  formData: VisitFormData;
  onFormDataChange: (data: Partial<VisitFormData>) => void;
  client: Client | null;
  animal: ClientAnimal | null;
  isRtl?: boolean;
}

// Visit type options with styling
const visitTypeOptions = [
  {
    value: VisitType.Clinic,
    labelKey: "visit.clinic",
    icon: Building2,
    color: "bg-blue-100 text-blue-700 border-blue-200",
    badgeColor: "bg-blue-100 text-blue-700",
  },
  {
    value: VisitType.Field,
    labelKey: "visit.field",
    icon: MapPin,
    color: "bg-emerald-100 text-emerald-700 border-emerald-200",
    badgeColor: "bg-emerald-100 text-emerald-700",
  },
  {
    value: VisitType.Emergency,
    labelKey: "visit.emergency",
    icon: AlertTriangle,
    color: "bg-red-100 text-red-700 border-red-200",
    badgeColor: "bg-red-100 text-red-700",
  },
];

// Payment status options with styling
const paymentStatusOptions = [
  {
    value: PaymentStatus.Pending,
    labelKey: "visit.paymentStatusPending",
    color: "bg-amber-100 text-amber-700 border-amber-200",
    badgeColor: "bg-amber-100 text-amber-700",
  },
  {
    value: PaymentStatus.Paid,
    labelKey: "visit.paymentStatusPaid",
    color: "bg-green-100 text-green-700 border-green-200",
    badgeColor: "bg-green-100 text-green-700",
  },
  {
    value: PaymentStatus.PartiallyPaid,
    labelKey: "visit.paymentStatusPartiallyPaid",
    color: "bg-blue-100 text-blue-700 border-blue-200",
    badgeColor: "bg-blue-100 text-blue-700",
  },
  {
    value: PaymentStatus.Refunded,
    labelKey: "visit.paymentStatusRefunded",
    color: "bg-slate-100 text-slate-700 border-slate-200",
    badgeColor: "bg-slate-100 text-slate-700",
  },
];

export default function VisitDetailsStep({
  formData,
  onFormDataChange,
  client,
  animal,
  isRtl = false,
}: VisitDetailsStepProps) {
  const { t } = useTranslation();

  const selectedTypeOption = visitTypeOptions.find(
    (opt) => opt.value === formData.visitType
  );

  const selectedPaymentStatusOption = paymentStatusOptions.find(
    (opt) => opt.value === formData.paymentStatus
  );

  return (
    <div className="space-y-5">
      {/* Client & Animal Summary Card */}
      {client && animal && (
        <div className="rounded-lg bg-slate-50 p-4 space-y-3">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {t(i18nKeyContainer.visit.visitInfo)}
          </p>

          {/* Client Info */}
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-200">
              <User className="h-4 w-4 text-slate-600" />
            </div>
            <div>
              <p className="text-sm font-medium text-slate-900">
                {client.fullName}
              </p>
              <p className="text-xs text-slate-500">
                {t(i18nKeyContainer.visit.owner)}
              </p>
            </div>
          </div>

          {/* Animal Info */}
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10">
              <PawPrint className="h-4 w-4 text-primary" />
            </div>
            <div>
              <p className="text-sm font-medium text-slate-900">
                {animal.name}
                <span className="ms-1.5 text-slate-500">• {animal.species}</span>
              </p>
              <p className="text-xs text-slate-500">
                {t(i18nKeyContainer.visit.animal)}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Visit Type Selection */}
      <div className="space-y-3 flex flex-col gap-1">
        <Label className="text-sm font-medium text-slate-700 mb-1">
          <div className="flex items-center gap-2">
            <Stethoscope className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.visit.visitType)}
            <span className="text-red-500">*</span>
          </div>
        </Label>
        <Select
          value={formData.visitType ? formData.visitType.toString() : ""}
          onValueChange={(value) =>
            onFormDataChange({ visitType: parseInt(value) })
          }
          dir={isRtl ? "rtl" : "ltr"}
        >
          <SelectTrigger className="h-11 border-slate-200 cursor-pointer">
            <SelectValue placeholder={t(i18nKeyContainer.visit.selectVisitType)}>
              {selectedTypeOption && (
                <div className="flex items-center gap-2">
                  <Badge
                    variant="outline"
                    className={`${selectedTypeOption.badgeColor} border-0 font-medium`}
                  >
                    <selectedTypeOption.icon className="h-3 w-3 me-1" />
                    {t(selectedTypeOption.labelKey)}
                  </Badge>
                </div>
              )}
            </SelectValue>
          </SelectTrigger>
          <SelectContent className="bg-white border-0 shadow-lg">
            {visitTypeOptions.map((option) => (
              <SelectItem
                key={option.value}
                value={option.value.toString()}
                className="cursor-pointer hover:bg-slate-100 focus:bg-slate-100"
              >
                <div className="flex items-center gap-2">
                  <Badge
                    variant="outline"
                    className={`${option.badgeColor} border-0 font-medium`}
                  >
                    <option.icon className="h-3 w-3 me-1" />
                    {t(option.labelKey)}
                  </Badge>
                </div>
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Symptoms */}
      <div className="space-y-3 flex flex-col gap-1">
        <Label
          htmlFor="symptoms"
          className="text-sm font-medium text-slate-700 mb-1"
        >
          <div className="flex items-center gap-2">
            <FileText className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.visit.symptoms)}
            <span className="text-xs text-slate-400">
              ({t(i18nKeyContainer.visit.optional)})
            </span>
          </div>
        </Label>
        <textarea
          id="symptoms"
          value={formData.symptoms}
          onChange={(e) => onFormDataChange({ symptoms: e.target.value })}
          placeholder={t(i18nKeyContainer.visit.symptomsPlaceholder)}
          className="min-h-20 w-full rounded-md border border-slate-200 bg-white px-3 py-2.5 text-sm placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary resize-none"
          dir={isRtl ? "rtl" : "ltr"}
        />
        <p className="text-xs text-slate-400 mt-1">
          {t(i18nKeyContainer.visit.commaSeparatedHint)}
        </p>
      </div>

      {/* Diagnosis */}
      <div className="space-y-3 flex flex-col gap-1">
        <Label
          htmlFor="diagnosis"
          className="text-sm font-medium text-slate-700 mb-1"
        >
          <div className="flex items-center gap-2">
            <Stethoscope className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.visit.diagnosis)}
            <span className="text-xs text-slate-400">
              ({t(i18nKeyContainer.visit.optional)})
            </span>
          </div>
        </Label>
        <textarea
          id="diagnosis"
          value={formData.diagnosis}
          onChange={(e) => onFormDataChange({ diagnosis: e.target.value })}
          placeholder={t(i18nKeyContainer.visit.diagnosisPlaceholder)}
          className="min-h-20 w-full rounded-md border border-slate-200 bg-white px-3 py-2.5 text-sm placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary resize-none"
          dir={isRtl ? "rtl" : "ltr"}
        />
      </div>

      {/* Treatment */}
      <div className="space-y-3 flex flex-col gap-1">
        <Label
          htmlFor="treatment"
          className="text-sm font-medium text-slate-700 mb-1"
        >
          <div className="flex items-center gap-2">
            <Pill className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.visit.treatment)}
            <span className="text-xs text-slate-400">
              ({t(i18nKeyContainer.visit.optional)})
            </span>
          </div>
        </Label>
        <textarea
          id="treatment"
          value={formData.treatment}
          onChange={(e) => onFormDataChange({ treatment: e.target.value })}
          placeholder={t(i18nKeyContainer.visit.treatmentPlaceholder)}
          className="min-h-20 w-full rounded-md border border-slate-200 bg-white px-3 py-2.5 text-sm placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary resize-none"
          dir={isRtl ? "rtl" : "ltr"}
        />
      </div>

      {/* Notes */}
      <div className="space-y-3 flex flex-col gap-1">
        <Label htmlFor="notes" className="text-sm font-medium text-slate-700 mb-1">
          <div className="flex items-center gap-2">
            <FileText className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.visit.notes)}
            <span className="text-xs text-slate-400">
              ({t(i18nKeyContainer.visit.optional)})
            </span>
          </div>
        </Label>
        <textarea
          id="notes"
          value={formData.notes}
          onChange={(e) => onFormDataChange({ notes: e.target.value })}
          placeholder={t(i18nKeyContainer.visit.notesPlaceholder)}
          className="min-h-20 w-full rounded-md border border-slate-200 bg-white px-3 py-2.5 text-sm placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary resize-none"
          dir={isRtl ? "rtl" : "ltr"}
        />
      </div>

      {/* Payment Information Section */}
      <div className="rounded-lg border border-slate-200 p-4 space-y-4">
        <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
          {t(i18nKeyContainer.visit.paymentInfo)}
        </p>

        {/* Payment Amount */}
        <div className="space-y-3 flex flex-col gap-1">
          <Label
            htmlFor="paymentAmount"
            className="text-sm font-medium text-slate-700 mb-1"
          >
            <div className="flex items-center gap-2">
              <DollarSign className="h-4 w-4 text-slate-400" />
              {t(i18nKeyContainer.visit.paymentAmount)}
              <span className="text-red-500">*</span>
            </div>
          </Label>
          <Input
            id="paymentAmount"
            type="number"
            min="0"
            step="0.01"
            value={formData.paymentAmount}
            onChange={(e) =>
              onFormDataChange({ paymentAmount: parseFloat(e.target.value) || 0 })
            }
            className="h-11 border-slate-200"
            dir={isRtl ? "rtl" : "ltr"}
          />
        </div>

        {/* Payment Status */}
        <div className="space-y-3 flex flex-col gap-1">
          <Label className="text-sm font-medium text-slate-700 mb-1">
            <div className="flex items-center gap-2">
              <CreditCard className="h-4 w-4 text-slate-400" />
              {t(i18nKeyContainer.visit.paymentStatus)}
              <span className="text-red-500">*</span>
            </div>
          </Label>
          <Select
            value={formData.paymentStatus ? formData.paymentStatus.toString() : ""}
            onValueChange={(value) =>
              onFormDataChange({ paymentStatus: parseInt(value) })
            }
            dir={isRtl ? "rtl" : "ltr"}
          >
            <SelectTrigger className="h-11 border-slate-200 cursor-pointer">
              <SelectValue placeholder={t(i18nKeyContainer.visit.selectPaymentStatus)}>
                {selectedPaymentStatusOption && (
                  <div className="flex items-center gap-2">
                    <Badge
                      variant="outline"
                      className={`${selectedPaymentStatusOption.badgeColor} border-0 font-medium`}
                    >
                      {t(selectedPaymentStatusOption.labelKey)}
                    </Badge>
                  </div>
                )}
              </SelectValue>
            </SelectTrigger>
            <SelectContent className="bg-white border-0 shadow-lg">
              {paymentStatusOptions.map((option) => (
                <SelectItem
                  key={option.value}
                  value={option.value.toString()}
                  className="cursor-pointer hover:bg-slate-100 focus:bg-slate-100"
                >
                  <div className="flex items-center gap-2">
                    <Badge
                      variant="outline"
                      className={`${option.badgeColor} border-0 font-medium`}
                    >
                      {t(option.labelKey)}
                    </Badge>
                  </div>
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
    </div>
  );
}
