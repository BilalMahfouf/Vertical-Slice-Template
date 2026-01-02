import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useTranslation } from "react-i18next";

export default function Login(){
    const { t } = useTranslation();
    return <div>{t(i18nKeyContainer.welcomeMessage)}</div>;
}