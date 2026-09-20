import { colors } from "@/constants/theme";
import type { PropsWithChildren } from "react";
import { SafeAreaView, ScrollView, StyleSheet, View } from "react-native";

type AppScreenProps = PropsWithChildren<{
  padded?: boolean;
}>;

export default function AppScreen({ children, padded = true }: AppScreenProps) {
  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        contentContainerStyle={[styles.content, padded ? styles.padded : null]}
        showsVerticalScrollIndicator={false}
      >
        <View>{children}</View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    backgroundColor: colors.background,
    flex: 1,
  },
  content: {
    flexGrow: 1,
    paddingBottom: 28,
  },
  padded: {
    paddingHorizontal: 20,
    paddingTop: 18,
  },
});
