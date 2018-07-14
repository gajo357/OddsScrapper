library(ggplot2)
library(dplyr)

setwd("C:/Users/Gajo/Documents/Visual Studio 2017/Projects/OddsScrapper/OddsScraper.Analysis")

data <- read.csv("soccer_england_premier-league.csv")

data$HomeOdd <- 1/data$HomeOdd
data$DrawOdd <- 1/data$DrawOdd
data$AwayOdd <- 1/data$AwayOdd

data$Date <- as.POSIXct(data$Date, format = "%m/%d/%Y %H:%M:%S")
data$IsValid <- as.logical(data$IsValid)
data$IsPlayoffs <- as.logical(data$IsPlayoffs)
data$IsOvertime <- as.logical(data$IsOvertime)

data$TotalGoals <- with(data, HomeTeamScore + AwayTeamScore)

data$HomeCut <- cut(data$HomeOdd,
                    breaks = seq(0, 1, .05),
                    right = TRUE)
data$DrawCut <- cut(data$DrawOdd,
                    breaks = seq(0, 1, .05),
                    right = TRUE)
data$AwayCut <- cut(data$AwayOdd,
                    breaks = seq(0, 1, .05),
                    right = TRUE)

summary(data)
summary(subset(data, IsValid))
summary(subset(data, Bookmaker == "bet365"))

summary(data$TotalGoals)

# Some plots
#############################

ggplot(subset(data, Bookmaker == "bet365"), 
       aes(AwayOdd, HomeOdd)) +
  geom_line(aes(color = IsValid)) +
  facet_wrap(~Season)


ggplot(data, 
       aes(AwayOdd, HomeOdd)) +
  geom_line(aes(color = IsValid)) +
  facet_wrap(~Season)

ggplot(data, 
       aes(HomeOdd)) +
  geom_histogram() +
  facet_wrap(~Season)


ggplot(data,
       aes(HomeOdd, AwayOdd)) +
  geom_point(alpha = 1/100) +
  facet_wrap(~Season)

ggplot(bet365, 
       aes(DrawCut)) +
  geom_histogram(stat = 'count')

with(data, cor.test(HomeOdd, AwayOdd))

bet365 <- subset(data, Bookmaker == "bet365")
sum(bet365$HomeOdd >= .5 & bet365$Season == "2017/2018")
sum(with(bet365, HomeOdd >= .5 & Season == "2017/2018" & HomeTeamScore > AwayTeamScore))

winProbability <- function(dataset) {
  noWins <- sum(with(dataset, HomeTeamScore > AwayTeamScore))
  noGames <- nrow(dataset)
  noWins/noGames
}

winProbability(subset(bet365, HomeOdd >= .6 & HomeOdd <= .7 & (Season %in% c("2017/2018"))))


# analysis by Game
###############################
dataByGame <- data %>%
  subset(IsValid) %>%
  group_by(HomeTeam, AwayTeam, Date, Season, HomeTeamScore, AwayTeamScore) %>%
  summarise(HomeOddMean = mean(HomeOdd),
            DrawOddMean = mean(DrawOdd),
            AwayOddMean = mean(AwayOdd),
            HomeOddMedian = median(HomeOdd),
            DrawOddMedian = median(DrawOdd),
            AwayOddMedian = median(AwayOdd),
            HomeOddDiff = max(HomeOdd) - min(HomeOdd),
            DrawOddDiff = max(DrawOdd) - min(DrawOdd),
            AwayOddDiff = max(AwayOdd) - min(AwayOdd),
            HomeWin = min(HomeTeamScore) > min(AwayTeamScore),
            DrawWin = min(HomeTeamScore) == min(AwayTeamScore),
            AwayWin = min(HomeTeamScore) < min(AwayTeamScore),
            n = n()) %>%
  arrange(Date)

dataByGame$HomeCut <- cut(dataByGame$HomeOddMean,
                    breaks = seq(0, 1, .1),
                    right = TRUE)
dataByGame$DrawCut <- cut(dataByGame$DrawOddMean,
                    breaks = seq(0, 1, .1),
                    right = TRUE)
dataByGame$AwayCut <- cut(dataByGame$AwayOddMean,
                    breaks = seq(0, 1, .1),
                    right = TRUE)

fit3 <- with(subset(data, Bookmaker == "bet365"), lm(HomeOdd ~ poly(DrawOdd, 3, raw = TRUE)))

with(subset(data, Bookmaker == "bet365"), 
     points(DrawOdd, predict(fit3, data.frame(x=DrawOdd)), col='red'))

ggplot(dataByGame,
       aes(HomeOddMean, DrawOddMean)) +
  geom_point(alpha = 1/20) + 
  geom_smooth(method = 'lm', formula = y~poly(x, 3, raw = TRUE), color = 'red') +
  facet_wrap(~Season)


# analysis by Cut
################################

dataByCut <- dataByGame %>%
  group_by(HomeCut, Season) %>%
  summarise(HomeOdd = mean(HomeOddMean),
            DrawOdd = mean(DrawOddMean),
            AwayOdd = mean(AwayOddMean),
            HomeWinPct = sum(HomeWin)/n(),
            DrawWinPct = sum(DrawWin)/n(),
            AwayWinPct = sum(AwayWin)/n(),
            HomeOddDiff = mean(HomeOddDiff),
            DrawOddDiff = mean(DrawOddDiff),
            AwayOddDiff = mean(AwayOddDiff),
            n = n(),
            HomeTeamScore = mean(HomeTeamScore),
            AwayTeamScore = mean(AwayTeamScore))
  
ggplot(dataByCut,
       aes(HomeWinPct, HomeOddDiff)) +
  geom_point(alpha = 1/3) + 
  geom_smooth(method = 'lm', color = 'pink') +
  geom_abline(slope = 1, intercept = 0 , color = 'red') +
  xlim(0, 1) +
  ylim(0, 1) +
  facet_wrap(~Season)

ggplot(dataByCut,
       aes(HomeWinPct, HomeOddDiff)) +
  geom_point(alpha = 1/3) + 
  geom_smooth(method = 'lm', color = 'pink') +
  ylim(0, .2) +
  facet_wrap(~Season)


with(dataByCut, cor.test(HomeOdd, HomeWinPct))
0.8861025

calculate.balance <- function(isWin, odd, kelly, balance){
  bet <- balance * kelly
  if (isWin) {
    win <- 1/odd - 1
  } else {
    win <- -1
  }
  balance <- balance + bet * win
  balance
}
brute.force.balance <- function(data, balance) {
  for(i in 1:nrow(good)) {
    row <- good[i,]
    balance <- calculate.balance(row$HomeTeamScore > row$AwayTeamScore, row$HomeOdd, row$Kelly, balance)
  }
  balance
}

good <- subset(bet365, Season == "2017/2018" & HomeOdd > 0.55) %>% arrange(Date)
good$Kelly <- (1/0.98 - 1)/(1/good$HomeOdd - 1)
brute.force.balance(good, 100)x
