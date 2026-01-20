import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus, Stethoscope } from "lucide-react";
import { Button } from "@/components/ui/button";
import VisitDataTable from "./visit-data-table";
import AddVisit from "./add-visit";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export default function VisitPage() {
  const [addVisitOpen, setAddVisitOpen] = useState(false);
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  const handleOpenAdd = () => {
    setAddVisitOpen(true);
  };

  const handleCloseAdd = () => {
    setAddVisitOpen(false);
  };

  return (
    <div dir={isRtl ? "rtl" : "ltr"}>
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-4 mb-6">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
            <Stethoscope className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">
              {t(i18nKeyContainer.visit.title)}
            </h1>
            <p className="text-slate-500">
              {t(i18nKeyContainer.visit.description)}
            </p>
          </div>
        </div>
        <Button
          className="sm:ms-auto gap-2 cursor-pointer w-full sm:w-auto"
          onClick={handleOpenAdd}
        >
          <Plus className="h-4 w-4" />
          {t(i18nKeyContainer.visit.addVisit)}
        </Button>
      </div>

      {/* Data Table */}
      <div>
        <VisitDataTable />
      </div>

      {/* Add Visit Modal */}
      <AddVisit open={addVisitOpen} onClose={handleCloseAdd} />
    </div>
  );
}