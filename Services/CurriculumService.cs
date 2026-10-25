using ARALReadingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ARALReadingSystem.Services
{
    public static class CurriculumService
    {
        private static readonly List<LessonSession> _sessions =
            BuildCurriculum();

        public static IReadOnlyList<LessonSession> GetAllSessions()
        {
            return _sessions;
        }

        public static LessonSession GetSession(int sessionNumber)
        {
            LessonSession? session =
                _sessions.FirstOrDefault(
                    s => s.SessionNumber == sessionNumber);

            if (session == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sessionNumber),
                    "Session number must be from 1 to 36.");
            }

            return session;
        }

        public static string GetSessionTitle(int sessionNumber)
        {
            return GetSession(sessionNumber).Title;
        }

        public static bool IsCheckIn(int sessionNumber)
        {
            return GetSession(sessionNumber).IsCheckIn;
        }

        private static List<LessonSession> BuildCurriculum()
        {
            List<LessonSession> sessions =
                new List<LessonSession>();

            // =====================================================
            // SESSION 1
            // AKO SI...
            // =====================================================

            sessions.Add(
                new LessonSession
                {
                    SessionNumber = 1,
                    Title = "Ako Si...",
                    ShortTitle = "Ako Si...",
                    Description = "Pagkilala sa sarili",
                    IsCheckIn = false,

                    Steps = new List<LessonStep>
                    {
                        new LessonStep
                        {
                            StepNumber = 1,
                            SectionName = "PAMBUNGAD",
                            Title = "Ako Si...",
                            Instruction =
                                "Kilalahon naton an aton kalugaringon.",
                            Prompt =
                                "Mag-andam para han aton buruhaton.",
                            VoiceoverText =
                                "Magkilala kita han aton kalugaringon.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 2,
                            SectionName = "PAGGUHIT",
                            Title = "Iguhit ang iyong sarili",
                            Instruction =
                                "Iguhit ang iyong sarili sa loob ng kahon. " +
                                "Maaari itong kulayan upang mas mapaganda!",
                            Prompt =
                                "Gamitin ang kahon bilang iyong drawing area.",
                            VoiceoverText =
                                "Iguhit an imo kalugaringon ha sulod han kahon.",
                            StepType =
                                LessonStepType.Drawing
                        },

                        new LessonStep
                        {
                            StepNumber = 3,
                            SectionName = "PAGKILALA",
                            Title = "Ako si...",
                            Instruction =
                                "Sagutan ang hinihinging impormasyon.",
                            Prompt = "Ako si",
                            Placeholder =
                                "Ibutang an ngaran han mag-aaral",
                            VoiceoverText =
                                "Isumat an imo ngaran.",
                            StepType =
                                LessonStepType.TextInput
                        },

                        new LessonStep
                        {
                            StepNumber = 4,
                            SectionName = "PAGKILALA",
                            Title = "Mahilig ako mag/sa...",
                            Instruction =
                                "Sagutan ang hinihinging impormasyon.",
                            Prompt =
                                "Mahilig ako mag/sa",
                            Placeholder =
                                "Ibutang an hilig han mag-aaral",
                            VoiceoverText =
                                "Ano an imo hilig buhaton?",
                            StepType =
                                LessonStepType.TextInput
                        },

                        new LessonStep
                        {
                            StepNumber = 5,
                            SectionName = "PAGTAPOS",
                            Title = "Maupay!",
                            Instruction =
                                "Repasuhon an imo buruhaton antes tapuson an sesyon.",
                            Prompt =
                                "Andam na nga tapuson an Sesyon 1.",
                            VoiceoverText =
                                "Maupay! Natapos mo an buruhaton.",
                            StepType =
                                LessonStepType.Review
                        }
                    }
                });

            // =====================================================
            // SESSION 2
            // TAYO AY...
            // =====================================================

            sessions.Add(
                new LessonSession
                {
                    SessionNumber = 2,
                    Title = "Tayo ay...",
                    ShortTitle = "Tayo ay...",
                    Description =
                        "Story Time Rules at pagsunod sa tuntunin",
                    IsCheckIn = false,

                    Steps = new List<LessonStep>
                    {
                        new LessonStep
                        {
                            StepNumber = 1,
                            SectionName = "PAMBUNGAD",
                            Title = "Tayo ay...",
                            Instruction =
                                "Mag-andam kita para ha Story Time.",
                            Prompt =
                                "Hibaruan naton an mga tuntunin para ha maupay nga pagpamati.",
                            VoiceoverText =
                                "Mag-andam kita para ha Story Time.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 2,
                            SectionName = "MGA TUNTUNIN",
                            Title = "Story Time Rules!",
                            Instruction =
                                "Para sa maayos na pakikinig sa isang kuwento, huwag kalimutan ang sumusunod:",
                            VoiceoverText =
                                "Pamatii an mga tuntunin para ha maupay nga Story Time.",
                            StepType =
                                LessonStepType.Rules,

                            Items = new List<string>
                            {
                                "Bukas na tainga sa pakikinig",
                                "Tikom na bibig upang tahimik",
                                "Kamay sa sarili at hindi nangangalabit",
                                "Bukas na isipan, imahinasyon ay handa na!"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 3,
                            SectionName = "GAWAIN 1",
                            Title = "Sumusunod ba sa tuntunin?",
                            Instruction =
                                "Tingnan nang mabuti ang mga larawan. " +
                                "Sila ba ay nagpapakita ng pagsunod sa tuntunin sa klase o hindi?",
                            Prompt =
                                "Piliin ang Oo kung sumusunod sa tuntunin at Hindi kung hindi.",
                            VoiceoverText =
                                "Pilia kon an ginbubuhat nasunod ha tuntunin o diri.",
                            StepType =
                                LessonStepType.ChoiceActivity,

                            Choices = new List<LessonChoice>
                            {
                                new LessonChoice
                                {
                                    Text =
                                        "Pagtataas ng kamay nang tahimik para sumagot kay teacher",
                                    CorrectAnswer = true
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "Pang-aasar sa kaklase",
                                    CorrectAnswer = false
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "Pananakit sa kaklase",
                                    CorrectAnswer = false
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "Pagtulong sa kaklase o kaibigan",
                                    CorrectAnswer = true
                                }
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 4,
                            SectionName = "PAGBALIK-ARAL",
                            Title = "Maupay!",
                            Instruction =
                                "Hinumdumi an mga tuntunin ha Story Time.",
                            Prompt =
                                "Maupay nga pagpamati ngan maupay nga pamatasan ha klase.",
                            VoiceoverText =
                                "Hinumdumi pirme an mga tuntunin ha klase.",
                            StepType =
                                LessonStepType.Review
                        },

                        new LessonStep
                        {
                            StepNumber = 5,
                            SectionName = "PAGTAPOS",
                            Title = "Natapos mo an Sesyon 2!",
                            Instruction =
                                "Andam ka na para ha sunod nga sesyon.",
                            Prompt =
                                "Kumpletohon an sesyon basi ma-save an progreso.",
                            VoiceoverText =
                                "Maupay! Natapos mo an Sesyon 2.",
                            StepType =
                                LessonStepType.Review
                        }
                    }
                });

            // =====================================================
            // SESSION 3
            // KANAN-KALIWA
            // =====================================================

            sessions.Add(
                new LessonSession
                {
                    SessionNumber = 3,
                    Title = "Kanan-Kaliwa",
                    ShortTitle = "Kanan-Kaliwa",
                    Description =
                        "Pagkilala sa kanan at kaliwa",
                    IsCheckIn = false,

                    Steps = new List<LessonStep>
                    {
                        new LessonStep
                        {
                            StepNumber = 1,
                            SectionName = "PAMBUNGAD",
                            Title = "Kanan-Kaliwa",
                            Instruction =
                                "Kilalahon naton an kaliwa ngan kanan.",
                            Prompt =
                                "Andam ka na? Gamita naton an aton mga kamot!",
                            VoiceoverText =
                                "Kilalahon naton an kaliwa ngan kanan.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 2,
                            SectionName = "KANAN-KALIWA",
                            Title =
                                "Alin ang kaliwa at alin ang kanan?",
                            Instruction =
                                "Kilalanin muna ang kaliwang kamay at kanang kamay.",
                            Prompt =
                                "Kaliwa — kaliwang kamay. Kanan — kanang kamay.",
                            VoiceoverText =
                                "Ini an wala nga kamot ngan ini an tuo nga kamot.",
                            StepType =
                                LessonStepType.HandColoring,
                            IsRequired = false
                        },

                        new LessonStep
                        {
                            StepNumber = 3,
                            SectionName = "GAWAIN 1",
                            Title =
                                "Kulayan ang mga kamay",
                            Instruction =
                                "Kulayan ng pula ang kaliwang kamay. " +
                                "Kulayan ng asul ang kanang kamay.",
                            Prompt =
                                "Piliin ang tamang kulay para sa bawat kamay.",
                            VoiceoverText =
                                "Kulayi hin pula an wala nga kamot ngan asul an tuo nga kamot.",
                            StepType =
                                LessonStepType.HandColoring,
                            IsRequired = true
                        },

                        new LessonStep
                        {
                            StepNumber = 4,
                            SectionName =
                                "PAGTITIBAY SA BAHAY",
                            Title =
                                "Kaliwa at kanan sa inyong kainan",
                            Instruction =
                                "Masdan ang kainan sa inyong bahay. " +
                                "Iguhit ang 3 bagay na nakikita sa kaliwa ng kainan " +
                                "at 3 bagay na nakikita sa kanan ng kainan. " +
                                "Kulayan ang mga ito.",
                            Prompt =
                                "Gawin ang pagsasanay na ito sa bahay kasama ang tagapag-alaga.",
                            VoiceoverText =
                                "Ha balay, kitaa an kainan ngan iguhit an mga butang ha wala ngan ha tuo.",
                            StepType =
                                LessonStepType.HomeReinforcement,
                            IsRequired = false
                        },

                        new LessonStep
                        {
                            StepNumber = 5,
                            SectionName = "PAGTAPOS",
                            Title =
                                "Maupay! Kanan at kaliwa!",
                            Instruction =
                                "Hinumdumi: pula para sa kaliwang kamay at asul para sa kanang kamay.",
                            Prompt =
                                "Andam na nga tapuson an Sesyon 3.",
                            VoiceoverText =
                                "Maupay! Nahibaro ka han wala ngan tuo.",
                            StepType =
                                LessonStepType.Review
                        }
                    }
                });

            // =====================================================
            // SESSION 4
            // PAKIKINIG SA TUNOG
            // =====================================================

            sessions.Add(
                new LessonSession
                {
                    SessionNumber = 4,
                    Title = "Pakikinig sa Tunog",
                    ShortTitle = "Pakikinig sa Tunog",
                    Description =
                        "Pakikinig at pagkilala sa magkatugmang tunog",
                    IsCheckIn = false,

                    Steps = new List<LessonStep>
                    {
                        new LessonStep
                        {
                            StepNumber = 1,
                            SectionName = "PAMBUNGAD",
                            Title = "Pakikinig sa Tunog",
                            Instruction =
                                "Mamati kita hin maupay ha tunog han mga pulong.",
                            Prompt =
                                "Pamatia kon magkapareho an tunog ha katapusan han duha nga pulong.",
                            VoiceoverText =
                                "Mamati kita hin maupay ha katapusan nga tunog han mga pulong.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 2,
                            SectionName = "MAGKATUGMA BA?",
                            Title =
                                "Pakinggan ang hulihang tunog",
                            Instruction =
                                "Basahin nang malakas ang dalawang larawan. " +
                                "Pakinggan nang mabuti ang hulihang tunog ng bawat salita.",
                            Prompt =
                                "Kung magkatunog ang hulihang tunog, piliin ang Oo. " +
                                "Kung hindi, piliin ang Hindi.",
                            VoiceoverText =
                                "Basaha an duha nga pulong ngan pamatii an ira katapusan nga tunog.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 3,
                            SectionName = "GAWAIN 1",
                            Title = "Magkatugma ba?",
                            Instruction =
                                "Basahin nang malakas ang dalawang larawan. " +
                                "Kung magkatunog ang hulihang tunog, piliin ang Oo. " +
                                "Kung hindi, piliin ang Hindi.",
                            Prompt =
                                "Pakinggan ang hulihang tunog ng bawat pares.",
                            VoiceoverText =
                                "Pamatia an katapusan nga tunog. Magkatugma ba?",
                            StepType =
                                LessonStepType.ChoiceActivity,

                            Choices = new List<LessonChoice>
                            {
                                new LessonChoice
                                {
                                    Text =
                                        "👶  BATA     +     🏀  BOLA",
                                    CorrectAnswer = false
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "🧹  WALIS     +     ✓  TSEK",
                                    CorrectAnswer = false
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "🧹  WALIS     +     👮  PULIS",
                                    CorrectAnswer = true
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "🍃  DAHON     +     📦  KAHON",
                                    CorrectAnswer = true
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "👄  BIBIG     +     🍚  KANIN",
                                    CorrectAnswer = false
                                },

                                new LessonChoice
                                {
                                    Text =
                                        "👶  BATA     +     👴  MATANDA",
                                    CorrectAnswer = true
                                }
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 4,
                            SectionName =
                                "PAGTITIBAY SA BAHAY",
                            Title =
                                "Magbasa mula kaliwa pakanan",
                            Instruction =
                                "Alalayan ang bata sa pagsasanay na magbasa mula kaliwa " +
                                "pakanan gamit ang mga numero, hugis, at larawan.",
                            Prompt =
                                "🌳   🏠   🚗   🐕   🍎\n\n" +
                                "■     ▲     ◆     ●     ▬\n\n" +
                                "5     7     4     1     3\n\n" +
                                "B     F     K     R     U",
                            VoiceoverText =
                                "Tikang ha wala pakadto ha tuo, basaha an mga larawan, " +
                                "hugis, numero, ngan letra.",
                            StepType =
                                LessonStepType.HomeReinforcement,
                            IsRequired = false
                        },

                        new LessonStep
                        {
                            StepNumber = 5,
                            SectionName =
                                "PAGTAPOS",
                            Title =
                                "Maupay nga pagpamati!",
                            Instruction =
                                "Nahibaro ka pagpamati kon magkatugma an hulihang tunog han mga pulong.",
                            Prompt =
                                "Hinumdumi: pamatia hin maupay an katapusan nga tunog.",
                            VoiceoverText =
                                "Maupay! Nahibaro ka pagpamati han magkatugmang tunog.",
                            StepType =
                                LessonStepType.Review
                        }
                    }
                });

            // =====================================================
            // SESSION 5
            // ALPABETONG FILIPINO
            // =====================================================

            sessions.Add(
                new LessonSession
                {
                    SessionNumber = 5,
                    Title = "Alpabetong Filipino",
                    ShortTitle = "Alpabetong Filipino",
                    Description =
                        "Pagkilala sa 28 titik ng Alpabetong Filipino",
                    IsCheckIn = false,

                    Steps = new List<LessonStep>
                    {
                        new LessonStep
                        {
                            StepNumber = 1,
                            SectionName = "PAMBUNGAD",
                            Title = "Alpabetong Filipino",
                            Instruction =
                                "Kilalanin natin ang Alpabetong Filipino.",
                            Prompt =
                                "Ang alpabetong Pilipino ay binubuo ng 28 na titik. " +
                                "Lima (5) ang patinig at 23 naman ang katinig.",
                            VoiceoverText =
                                "Kilalahon naton an mga letra han Alpabetong Filipino.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 2,
                            SectionName = "ALPABETO",
                            Title = "28 Titik",
                            Instruction =
                                "Basahin ang malaki at maliit na anyo ng bawat titik.",
                            Prompt =
                                "May 5 patinig at 23 katinig.",
                            VoiceoverText =
                                "Basahon naton an mga letra han alpabeto.",
                            StepType =
                                LessonStepType.AlphabetDisplay,

                            Items = new List<string>
                            {
                                "Aa", "Bb", "Cc", "Dd", "Ee", "Ff",
                                "Gg", "Hh", "Ii", "Jj", "Kk", "Ll",
                                "Mm", "Nn", "Ññ", "NGng", "Oo", "Pp",
                                "Qq", "Rr", "Ss", "Tt", "Uu", "Vv",
                                "Ww", "Xx", "Yy", "Zz"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 3,
                            SectionName = "GAWAIN 1",
                            Title =
                                "Malaki at maliit na titik",
                            Instruction =
                                "Pagtambalin ang malaki at maliit na titik.",
                            Prompt =
                                "Piliin ang katumbas na maliit na titik.",
                            VoiceoverText =
                                "Pagtambalon an dako ngan gutiay nga letra.",
                            StepType =
                                LessonStepType.LetterMatching,

                            Items = new List<string>
                            {
                                "B|b,u,f,r,m",
                                "F|u,f,r,m,b",
                                "M|f,r,m,b,u",
                                "R|r,m,b,u,f",
                                "U|m,b,u,f,r",

                                "Q|e,q,j,a,d",
                                "A|q,j,a,d,e",
                                "D|j,a,d,e,q",
                                "J|a,d,e,q,j",
                                "E|d,e,q,j,a"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 4,
                            SectionName = "GAWAIN 2",
                            Title = "Patinig",
                            Instruction =
                                "Makinig sa bibigkasin na pangalan ng larawan. " +
                                "Lagyan ng tsek ang larawan kung ito ay nagsisimula " +
                                "sa patinig (a, e, i, o, u).",
                            Prompt =
                                "Piliin ang mga salitang nagsisimula sa patinig.",
                            VoiceoverText =
                                "Pilia an mga pulong nga nagtitikang ha patinig.",
                            StepType =
                                LessonStepType.VowelActivity,

                            Items = new List<string>
                            {
                                "✏ LAPIS|false",
                                "🥬 UPO|true",
                                "🏠 KUBO|false",
                                "🕐 ORASAN|true",
                                "4  APAT|true",
                                "☕ KAPE|false",
                                "🥚 ITLOG|true",
                                "🌀 ELESI|true"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 5,
                            SectionName = "GAWAIN 3",
                            Title = "Katinig",
                            Instruction =
                                "Kulayan ang larawang nagsisimula sa katinig. " +
                                "Isulat ang titik sa patlang.",
                            Prompt =
                                "Piliin ang simulang titik ng bawat salitang nagsisimula sa katinig.",
                            VoiceoverText =
                                "Kilalaha an mga pulong nga nagtitikang ha katinig.",
                            StepType =
                                LessonStepType.ConsonantActivity,

                            Items = new List<string>
                            {
                                "🎀 LASO|L|true",
                                "☁ ULAP|U|false",
                                "🧺 BASKET|B|true",
                                "☀ ARAW|A|false",
                                "⛵ BANGKA|B|true",
                                "🍌 SAGING|S|true",
                                "🇵🇭 BANDILA|B|true",
                                "👀 MATA|M|true"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 6,
                            SectionName = "GAWAIN 4",
                            Title = "Nawawalang Titik",
                            Instruction =
                                "Isulat ang nawawalang patinig o katinig.",
                            Prompt =
                                "Kumpletuhin ang bawat salita.",
                            VoiceoverText =
                                "Isurat an nawawara nga letra.",
                            StepType =
                                LessonStepType.MissingLetterActivity,

                            Items = new List<string>
                            {
                                "🌳|_uno|P",
                                "🏠|_ubo|K",
                                "🥛|_aso|B",
                                "🐕|_so|A",
                                "🍎|_ansanas|M"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 7,
                            SectionName = "PAGTAPOS",
                            Title =
                                "Maupay! Alpabetong Filipino!",
                            Instruction =
                                "Nahibaro ka han mga titik, patinig, ngan katinig.",
                            Prompt =
                                "Andam na nga tapuson an Sesyon 5.",
                            VoiceoverText =
                                "Maupay! Natapos mo an Sesyon 5.",
                            StepType =
                                LessonStepType.Review
                        }
                    }
                });

            // =====================================================
            // SESSION 6
            // TITIK Mm
            // =====================================================

            sessions.Add(
                new LessonSession
                {
                    SessionNumber = 6,
                    Title = "Titik Mm",
                    ShortTitle = "Titik Mm",
                    Description =
                        "Pagkilala, pagsulat, at tunog ng titik Mm",
                    IsCheckIn = false,

                    Steps = new List<LessonStep>
                    {
                        new LessonStep
                        {
                            StepNumber = 1,
                            SectionName = "PAMBUNGAD",
                            Title = "Titik Mm",
                            Instruction =
                                "Kilalanin natin ang malaking M at maliit na m.",
                            Prompt =
                                "M • m — Pakinggan at bigkasin ang tunog /m/.",
                            VoiceoverText =
                                "Kilalahon naton an dako nga M ngan gutiay nga m. " +
                                "Pamatia ngan bigkasa an tunog mmm.",
                            StepType =
                                LessonStepType.Introduction
                        },

                        new LessonStep
                        {
                            StepNumber = 2,
                            SectionName =
                                "MAGSANAY MAGSULAT",
                            Title =
                                "Isulat ang M at m",
                            Instruction =
                                "Sundan ang hugis ng malaking M at maliit na m. " +
                                "Pagkatapos, magsanay magsulat ng M at m.",
                            Prompt =
                                "M   M   M     m   m   m",
                            VoiceoverText =
                                "Magsanay pagsurat han dako nga M ngan gutiay nga m.",
                            StepType =
                                LessonStepType.LetterWriting,
                            IsRequired = true,

                            Items = new List<string>
                            {
                                "M",
                                "m"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 3,
                            SectionName = "GAWAIN 1",
                            Title =
                                "Kulayan ang mais",
                            Instruction =
                                "Kulayan ng dilaw ang mais na may malaking titik M. " +
                                "Kulayan naman ng pula ang mais na may maliit na titik m.",
                            Prompt =
                                "M = Dilaw • m = Pula",
                            VoiceoverText =
                                "Kulayi hin dilaw an mais nga may dako nga M, " +
                                "ngan pula an mais nga may gutiay nga m.",
                            StepType =
                                LessonStepType.LetterColoring,
                            IsRequired = true,

                            Items = new List<string>
                            {
                                "M|yellow",
                                "B|none",
                                "M|yellow",
                                "M|yellow",
                                "m|red",
                                "b|none",
                                "m|red",
                                "M|yellow",
                                "K|none"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 4,
                            SectionName = "GAWAIN 2",
                            Title =
                                "Alin ang nagsisimula sa tunog /m/?",
                            Instruction =
                                "Bilugan ang larawan na ang pangalan ay nagsisimula " +
                                "sa tunog ng titik M.",
                            Prompt =
                                "Piliin ang mga larawang nagsisimula sa /m/.",
                            VoiceoverText =
                                "Pilia an mga larawan nga an ngaran nagtitikang ha tunog mmm.",
                            StepType =
                                LessonStepType.BeginningSoundActivity,
                            IsRequired = true,

                            Items = new List<string>
                            {
                                "🪑 MESA|true",
                                "👟 SAPATOS|false",
                                "🌽 MAIS|true",
                                "🏅 MEDALYA|true",
                                "🪆 MANIKA|true",
                                "🎁 REGALO|false",
                                "🐔 MANOK|true",
                                "🥜 MANI|true"
                            }
                        },

                        new LessonStep
                        {
                            StepNumber = 5,
                            SectionName = "PAGTAPOS",
                            Title =
                                "Maupay! Titik Mm!",
                            Instruction =
                                "Nahibaro ka pagkilala, pagsulat, ngan pagpamati " +
                                "han tunog han titik M at m.",
                            Prompt =
                                "M at m • tunog /m/",
                            VoiceoverText =
                                "Maupay! Nahibaro ka han titik M ngan han tunog mmm.",
                            StepType =
                                LessonStepType.Review
                        }
                    }
                });

            // =====================================================
            // SESSION 7+
            // =====================================================

            // =====================================================
// SESSION 7
// TITIK Ss
// =====================================================

sessions.Add(
    new LessonSession
    {
        SessionNumber = 7,
        Title = "Titik Ss",
        ShortTitle = "Titik Ss",
        Description =
            "Pagkilala, pagsulat, at tunog ng titik Ss",
        IsCheckIn = false,

        Steps = new List<LessonStep>
        {
            new LessonStep
            {
                StepNumber = 1,
                SectionName = "PAMBUNGAD",
                Title = "Titik Ss",

                Instruction =
                    "Kilalanin natin ang malaking S at maliit na s.",

                Prompt =
                    "S • s — Pakinggan at bigkasin ang tunog /s/.",

                VoiceoverText =
                    "Kilalahon naton an dako nga S ngan gutiay nga s. " +
                    "Pamatia ngan bigkasa an tunog sss.",

                StepType =
                    LessonStepType.Introduction
            },

            new LessonStep
            {
                StepNumber = 2,
                SectionName = "MAGSANAY MAGSULAT",
                Title = "Isulat ang S at s",

                Instruction =
                    "Sundan ang hugis ng malaking S at maliit na s. " +
                    "Pagkatapos, magsanay magsulat ng S at s.",

                Prompt =
                    "S   S   S     s   s   s",

                VoiceoverText =
                    "Magsanay pagsurat han dako nga S ngan gutiay nga s.",

                StepType =
                    LessonStepType.LetterWriting,

                IsRequired = true,

                Items = new List<string>
                {
                    "S",
                    "s"
                }
            },

            new LessonStep
            {
                StepNumber = 3,
                SectionName = "GAWAIN 4",
                Title =
                    "Alin ang nagsisimula sa tunog /s/?",

                Instruction =
                    "Bilugan ang mga bagay na ang pangalan " +
                    "ay nasisimula sa tunog na /s/.",

                Prompt =
                    "Piliin ang mga larawang nagsisimula sa /s/.",

                VoiceoverText =
                    "Pilia an mga butang nga an ngaran nagtitikang ha tunog sss.",

                StepType =
                    LessonStepType.BeginningSoundActivity,

                IsRequired = true,

                Items = new List<string>
                {
                    "🔑 SUSI|true",
                    "🌸 BULAKLAK|false",
                    "👟 SAPATOS|true",

                    "✏ LAPIS|false",
                    "🧼 SABON|true",
                    "📗 LIBRO|false",

                    "🌿 SAMPALOK|true",
                    "🧺 SAKO|true",
                    "🪞 SALAMIN|true"
                }
            },

            new LessonStep
            {
                StepNumber = 4,
                SectionName = "PAGBALIK-ARAL",
                Title = "Titik S at s",

                Instruction =
                    "Balikan natin ang malaking S, maliit na s, " +
                    "at ang tunog na /s/.",

                Prompt =
                    "S • s • /s/\n" +
                    "susi • sapatos • sabon • sampalok • sako • salamin",

                VoiceoverText =
                    "Hinumdumi an dako nga S, gutiay nga s, ngan tunog sss.",

                StepType =
                    LessonStepType.Review
            },

            new LessonStep
            {
                StepNumber = 5,
                SectionName = "PAGTAPOS",
                Title = "Maupay! Titik Ss!",

                Instruction =
                    "Nahibaro ka pagkilala, pagsulat, ngan pagpamati " +
                    "han tunog han titik S at s.",

                Prompt =
                    "S at s • tunog /s/",

                VoiceoverText =
                    "Maupay! Nahibaro ka han titik S ngan han tunog sss.",

                StepType =
                    LessonStepType.Review
            }
        }
    });

            // =====================================================
// SESSION 8
// TITIK Aa
// =====================================================

sessions.Add(
    new LessonSession
    {
        SessionNumber = 8,
        Title = "Titik Aa",
        ShortTitle = "Titik Aa",
        Description =
            "Pagkilala, pagsulat, at tunog ng titik Aa",
        IsCheckIn = false,

        Steps = new List<LessonStep>
        {
            // =================================================
            // STEP 1 - PAMBUNGAD
            // =================================================

            new LessonStep
            {
                StepNumber = 1,
                SectionName = "PAMBUNGAD",
                Title = "Titik Aa",

                Instruction =
                    "Kilalanin natin ang malaking A at maliit na a.",

                Prompt =
                    "A • a — Pakinggan at bigkasin ang tunog /a/.",

                VoiceoverText =
                    "Kilalahon naton an dako nga A ngan gutiay nga a. " +
                    "Pamatia ngan bigkasa an tunog aaa.",

                StepType =
                    LessonStepType.Introduction
            },

            // =================================================
            // STEP 2 - MAGSANAY MAGSULAT
            // =================================================

            new LessonStep
            {
                StepNumber = 2,
                SectionName = "MAGSANAY MAGSULAT",
                Title = "Isulat ang A at a",

                Instruction =
                    "Sundan ang hugis ng malaking A at maliit na a. " +
                    "Pagkatapos, magsanay magsulat ng A at a.",

                Prompt =
                    "A   A   A     a   a   a",

                VoiceoverText =
                    "Magsanay pagsurat han dako nga A ngan gutiay nga a.",

                StepType =
                    LessonStepType.LetterWriting,

                IsRequired = true,

                Items = new List<string>
                {
                    "A",
                    "a"
                }
            },

            // =================================================
            // STEP 3 - GAWAIN 1
            // =================================================

            new LessonStep
            {
                StepNumber = 3,
                SectionName = "GAWAIN 1",
                Title = "Malaking A at maliit na a",

                Instruction =
                    "Bilugan ang malaking titik A at lagyan ng tsek " +
                    "ang maliit na titik a sa bawat salita.",

                Prompt =
                    "Hanapin ang malaking A at maliit na a.",

                VoiceoverText =
                    "Bilugi an dako nga A ngan butangi hin tsek " +
                    "an gutiay nga a ha kada pulong.",

                StepType =
                    LessonStepType.LetterCaseActivity,

                IsRequired = true,

                Items = new List<string>
                {
                    "asa",
                    "Ana",
                    "isa",
                    "araw",
                    "ama",
                    "ale",
                    "Annie",
                    "ate"
                }
            },

            // =================================================
            // STEP 4 - GAWAIN 2
            // =================================================

            new LessonStep
            {
                StepNumber = 4,
                SectionName = "GAWAIN 2",
                Title =
                    "Alin ang nagsisimula sa tunog /a/?",

                Instruction =
                    "Bilugan ang mga bagay na ang pangalan ay " +
                    "nasisimula sa tunog na /a/.",

                Prompt =
                    "Piliin ang mga larawang nagsisimula sa /a/.",

                VoiceoverText =
                    "Pilia an mga butang nga an ngaran " +
                    "nagtitikang ha tunog aaa.",

                StepType =
                    LessonStepType.BeginningSoundActivity,

                IsRequired = true,

                Items = new List<string>
                {
                    "☀️ ARAW|true",
                    "🥚 ITLOG|false",
                    "🍦 SORBETES|false",

                    "🍈 ATIS|true",
                    "🦀 ALIMANGO|true",
                    "☁️ ULAP|false",

                    "🪭 ABANIKO|true",
                    "🐟 ISDA|false",
                    "🐕 ASO|true"
                }
            },

            // =================================================
            // STEP 5 - GAWAIN 3
            // =================================================

            new LessonStep
            {
                StepNumber = 5,
                SectionName = "GAWAIN 3",
                Title = "Simulang Titik",

                Instruction =
                    "Gumuhit ng linya papunta sa simulang " +
                    "titik ng larawan.",

                Prompt =
                    "Piliin kung a, m, o s ang simulang titik.",

                VoiceoverText =
                    "Pilia an simulang letra han kada larawan.",

                StepType =
                    LessonStepType.BeginningLetterActivity,

                IsRequired = true,

                Items = new List<string>
                {
                    "🍈 ATIS|a",
                    "🐔 MANOK|m",
                    "👟 SAPATOS|s",
                    "🦀 ALIMANGO|a"
                }
            },

            // =================================================
            // STEP 6 - PAGBALIK-ARAL
            // =================================================

            new LessonStep
            {
                StepNumber = 6,
                SectionName = "PAGBALIK-ARAL",
                Title = "Titik A at a",

                Instruction =
                    "Balikan natin ang malaking A, maliit na a, " +
                    "at ang tunog na /a/.",

                Prompt =
                    "A • a • /a/\n" +
                    "araw • atis • alimango • abaniko • aso",

                VoiceoverText =
                    "Hinumdumi an dako nga A, gutiay nga a, " +
                    "ngan tunog aaa.",

                StepType =
                    LessonStepType.Review
            },

            // =================================================
            // STEP 7 - PAGTAPOS
            // =================================================

            new LessonStep
            {
                StepNumber = 7,
                SectionName = "PAGTAPOS",
                Title = "Maupay! Titik Aa!",

                Instruction =
                    "Nahibaro ka pagkilala, pagsulat, ngan pagpamati " +
                    "han tunog han titik A at a.",

                Prompt =
                    "A at a • tunog /a/ • simulang titik",

                VoiceoverText =
                    "Maupay! Nahibaro ka han titik A " +
                    "ngan han tunog aaa.",

                StepType =
                    LessonStepType.Review
            }
        }
    });

            sessions.Add(CreatePracticeSession(
                9,
                "Pagsasanay (Titik M, S, A, at Pagpapantig)",
                "Pagkilala sa M, S, A at pagbuo ng pantig",
                "Pagsamahin ang tunog at titik upang mabasa ang pantig.",
                new[]
                {
                    "Alin ang tamang hati ng MAIS?|ma-is~mi-as~sa-im|ma-is",
                    "Aling pantig ang mabubuo sa M + A?|MA~SA~MI|MA",
                    "Alin ang tamang hati ng MESA?|me-sa~em-sa~ma-se|me-sa",
                    "Piliin ang pantig na nagsisimula sa tunog /s/.|SA~MA~AS|SA",
                    "Ilang pantig ang nasa salitang SAMA?|1~2~3|2"
                }));

            sessions.Add(CreatePracticeSession(
                10,
                "Pagsasanay (Titik M, S, A, at Salitandaan)",
                "Pagkilala at pagbasa ng mga salitang may M, S, at A",
                "Basahin ang bawat salita at piliin ang katugmang sagot.",
                new[]
                {
                    "Aling salita ang nagsisimula sa M?|MATA~SAMA~ASA|MATA",
                    "Aling salita ang may unang titik na S?|MESA~SUSI~AMA|SUSI",
                    "Aling salita ang nagsisimula sa A?|SAMA~ASO~MESA|ASO",
                    "Piliin ang salitang mabubuo sa MA + TA.|MATA~SATA~MAMA|MATA",
                    "Aling salita ang may titik M sa gitna?|ASA~SAMA~SUSI|SAMA"
                }));

            sessions.Add(CreateLetterLesson(
                11,
                "Titik Ii",
                "Kilalanin at isulat ang malaking I at maliit na i.",
                new[] { "I" },
                new[] { "ibon", "isda", "ilaw", "sisi", "Iisa" },
                new[]
                {
                    "🐦 IBON|true",
                    "💡 ILAW|true",
                    "🐟 ISDA|true",
                    "🥭 MANGGA|false",
                    "🏠 BAHAY|false"
                },
                new[]
                {
                    "🐦 IBON|I",
                    "💡 ILAW|I",
                    "🐟 ISDA|I",
                    "🥭 MANGGA|M"
                },
                "I,M,S"));

            sessions.Add(CreateLetterLesson(
                12,
                "Titik Oo",
                "Kilalanin at isulat ang malaking O at maliit na o.",
                new[] { "O" },
                new[] { "oso", "oras", "okra", "suko", "Oto" },
                new[]
                {
                    "🐻 OSO|true",
                    "⏰ ORAS|true",
                    "🥬 OKRA|true",
                    "🍇 UBAS|false",
                    "🪑 MESA|false"
                },
                new[]
                {
                    "🐻 OSO|O",
                    "⏰ ORAS|O",
                    "🥬 OKRA|O",
                    "🍇 UBAS|U"
                },
                "O,I,U"));

            sessions.Add(CreateLetterLesson(
                13,
                "Titik Ee",
                "Kilalanin at isulat ang malaking E at maliit na e.",
                new[] { "E" },
                new[] { "eroplano", "elepante", "ensaymada", "mesa", "Eba" },
                new[]
                {
                    "✈️ EROPLANO|true",
                    "🐘 ELEPANTE|true",
                    "🥮 ENSAYMADA|true",
                    "🐟 ISDA|false",
                    "🌽 MAIS|false"
                },
                new[]
                {
                    "✈️ EROPLANO|E",
                    "🐘 ELEPANTE|E",
                    "🥮 ENSAYMADA|E",
                    "🐟 ISDA|I"
                },
                "E,I,O"));

            sessions.Add(CreatePracticeSession(
                14,
                "Pagsasanay (Titik I, O, at E)",
                "Pagbasa at pagkilala sa mga patinig I, O, at E",
                "Piliin ang titik o salitang tumutugma sa tanong.",
                new[]
                {
                    "Anong unang titik ng IBON?|I~O~E|I",
                    "Anong unang titik ng OSO?|E~O~I|O",
                    "Aling salita ang nagsisimula sa E?|EROPLANO~ISDA~OKRA|EROPLANO",
                    "Piliin ang salitang may O sa hulihan.|SUSO~ILAW~IBON|SUSO",
                    "Aling patinig ang nasa gitna ng MESA?|A~E~I|E"
                }));

            sessions.Add(CreateCheckInSession(15, 1));

            sessions.Add(CreateLetterLesson(
                16,
                "Titik Bb",
                "Kilalanin at isulat ang malaking B at maliit na b.",
                new[] { "B" },
                new[] { "bata", "bola", "buko", "lobo", "Bebe" },
                new[]
                {
                    "⚽ BOLA|true",
                    "🥥 BUKO|true",
                    "👧 BATA|true",
                    "🧹 Walis|false",
                    "🐟 ISDA|false"
                },
                new[]
                {
                    "⚽ BOLA|B",
                    "🥥 BUKO|B",
                    "👧 BATA|B",
                    "👟 SAPATOS|S"
                },
                "B,M,S"));

            sessions.Add(CreateLetterLesson(
                17,
                "Titik Uu",
                "Kilalanin at isulat ang malaking U at maliit na u.",
                new[] { "U" },
                new[] { "ulo", "ubas", "upuan", "suso", "Ulan" },
                new[]
                {
                    "🍇 UBAS|true",
                    "🪑 UPUAN|true",
                    "🙂 ULO|true",
                    "🧢 SUMBRERO|false",
                    "⚽ BOLA|false"
                },
                new[]
                {
                    "🍇 UBAS|U",
                    "🪑 UPUAN|U",
                    "🙂 ULO|U",
                    "⚽ BOLA|B"
                },
                "U,B,O"));

            sessions.Add(CreatePracticeSession(
                18,
                "Salitandaan (May, Mga)",
                "Paggamit ng may at mga sa payak na pangungusap",
                "Piliin ang salitang babagay sa bilang ng bagay.",
                new[]
                {
                    "Aling pahayag ang may isang lapis?|May isang lapis~Mga lapis~Walang lapis|May isang lapis",
                    "Aling pahayag ang tumutukoy sa maraming bata?|May isang bata~Mga bata~Isang bata|Mga bata",
                    "Aling salita ang nagsasaad na may isang bola?|May~Mga~Wala|May",
                    "Aling parirala ang tumutukoy sa maraming aklat?|May aklat~Mga aklat~Isang aklat|Mga aklat",
                    "Aling pahayag ang may isang ibon?|May isang ibon~Mga ibon~Walang ibon|May isang ibon"
                }));

            sessions.Add(CreatePracticeSession(
                19,
                "Pagsasanay (Titik B, U, at Salitandaan)",
                "Pagkilala sa B at U at paggamit ng may at mga",
                "Basahin ang tanong at piliin ang tamang sagot.",
                new[]
                {
                    "Anong unang titik ng BOLA?|B~U~M|B",
                    "Anong unang titik ng ULO?|U~O~B|U",
                    "Aling parirala ang tumutukoy sa maraming ubas?|May ubas~Mga ubas~Isang ubas|Mga ubas",
                    "Aling parirala ang tumutukoy sa maraming bata?|May bata~Mga bata~Isang bata|Mga bata",
                    "Aling salita ang nagsisimula sa B?|UBAS~BATA~ULO|BATA"
                }));

            sessions.Add(CreateLetterLesson(
                20,
                "Titik Kk at Ll",
                "Kilalanin at isulat ang Kk at Ll.",
                new[] { "K", "L" },
                new[] { "kama", "kubo", "lobo", "lapis", "kalapati" },
                new[]
                {
                    "🛏️ KAMA|true",
                    "🏠 KUBO|true",
                    "✏️ LAPIS|true",
                    "🎈 LOBO|true",
                    "🍇 UBAS|false"
                },
                new[]
                {
                    "🛏️ KAMA|K",
                    "🏠 KUBO|K",
                    "✏️ LAPIS|L",
                    "🎈 LOBO|L"
                },
                "K,L,M"));

            sessions.Add(CreateLetterLesson(
                21,
                "Titik Tt at Nn",
                "Kilalanin at isulat ang Tt at Nn.",
                new[] { "T", "N" },
                new[] { "tasa", "tubig", "niyog", "nido", "tanim" },
                new[]
                {
                    "🥤 TASA|true",
                    "💧 TUBIG|true",
                    "🥥 NIYOG|true",
                    "🪺 NIDO|true",
                    "⚽ BOLA|false"
                },
                new[]
                {
                    "🥤 TASA|T",
                    "💧 TUBIG|T",
                    "🥥 NIYOG|N",
                    "🪺 NIDO|N"
                },
                "T,N,B"));

            sessions.Add(CreateLetterLesson(
                22,
                "Titik Ww at Yy",
                "Kilalanin at isulat ang Ww at Yy.",
                new[] { "W", "Y" },
                new[] { "walis", "watawat", "yelo", "yema", "yoyo" },
                new[]
                {
                    "🧹 WALIS|true",
                    "🇵🇭 WATAWAT|true",
                    "🍦 YELO|true",
                    "🍬 YEMA|true",
                    "🛏️ KAMA|false"
                },
                new[]
                {
                    "🧹 WALIS|W",
                    "🇵🇭 WATAWAT|W",
                    "🍦 YELO|Y",
                    "🍬 YEMA|Y"
                },
                "W,Y,L"));

            sessions.Add(CreatePracticeSession(
                23,
                "Pagsasanay (Titik T, K, L, N, W, at Y)",
                "Pagkilala sa mga titik T, K, L, N, W, at Y",
                "Piliin ang tamang unang titik ng bawat salita.",
                new[]
                {
                    "Anong titik ang simula ng TASA?|T~K~N|T",
                    "Anong titik ang simula ng KUBO?|L~K~W|K",
                    "Anong titik ang simula ng LAPIS?|Y~L~T|L",
                    "Anong titik ang simula ng NIYOG?|N~W~K|N",
                    "Anong titik ang simula ng WALIS?|W~Y~L|W",
                    "Anong titik ang simula ng YELO?|T~Y~N|Y"
                }));

            sessions.Add(CreateCheckInSession(24, 2));

            sessions.Add(CreateLetterLesson(
                25,
                "Titik Gg at NGng",
                "Kilalanin ang Gg at ang pinagsamang titik NGng.",
                new[] { "G", "NG" },
                new[] { "gatas", "gabi", "gulong", "puno", "rosas" },
                new[]
                {
                    "🥛 GATAS|true",
                    "🌙 GABI|true",
                    "😁 NGITI|true",
                    "🦷 NGIPIN|true",
                    "🛏️ KAMA|false"
                },
                new[]
                {
                    "🥛 GATAS|G",
                    "🌙 GABI|G",
                    "😁 NGITI|NG",
                    "🦷 NGIPIN|NG"
                },
                "G,NG,P",
                new[] { "G" },
                new[]
                {
                    "Aling salita ang nagsisimula sa NG?|NGITI~GABI~PUNO|NGITI",
                    "Aling salita ang nagsisimula sa G?|GATAS~NGIPIN~ROSA|GATAS"
                }));

            sessions.Add(CreateLetterLesson(
                26,
                "Titik Pp at Rr",
                "Kilalanin at isulat ang Pp at Rr.",
                new[] { "P", "R" },
                new[] { "puno", "palaka", "relo", "rosas", "parol" },
                new[]
                {
                    "🌳 PUNO|true",
                    "🐸 PALAKA|true",
                    "⌚ RELO|true",
                    "🌹 ROSAS|true",
                    "🧹 WALIS|false"
                },
                new[]
                {
                    "🌳 PUNO|P",
                    "🐸 PALAKA|P",
                    "⌚ RELO|R",
                    "🌹 ROSAS|R"
                },
                "P,R,G"));

            sessions.Add(CreatePracticeSession(
                27,
                "Pagsasanay (Titik G, NG, P at R)",
                "Pagkilala sa G, NG, P, at R sa unahan ng salita",
                "Piliin ang unang titik o pinagsamang titik.",
                new[]
                {
                    "Anong simula ng GATAS?|G~NG~P|G",
                    "Anong simula ng NGITI?|NG~G~R|NG",
                    "Anong simula ng PUNO?|P~R~NG|P",
                    "Anong simula ng RELO?|G~R~P|R",
                    "Aling salita ang may NG sa unahan?|NGIPIN~GULONG~ROSAS|NGIPIN"
                }));

            sessions.Add(CreateLetterLesson(
                28,
                "Titik Dd at Hh",
                "Kilalanin at isulat ang Dd at Hh.",
                new[] { "D", "H" },
                new[] { "dahon", "dila", "halaman", "hipon", "daga" },
                new[]
                {
                    "🍃 DAHON|true",
                    "👅 DILA|true",
                    "🌿 HALAMAN|true",
                    "🦐 HIPON|true",
                    "🥥 BUKO|false"
                },
                new[]
                {
                    "🍃 DAHON|D",
                    "👅 DILA|D",
                    "🌿 HALAMAN|H",
                    "🦐 HIPON|H"
                },
                "D,H,B"));

            sessions.Add(CreatePracticeSession(
                29,
                "Kambal Katinig / Klaster",
                "Pagkilala sa magkasunod na katinig sa mga salita",
                "Hanapin ang dalawang katinig na magkasunod sa salita.",
                new[]
                {
                    "Aling salita ang nagsisimula sa PL?|PLATO~PUNO~LAPIS|PLATO",
                    "Aling salita ang nagsisimula sa PR?|PRUTAS~ROSAS~TASA|PRUTAS",
                    "Aling salita ang nagsisimula sa TR?|TRAPO~DAGA~GABI|TRAPO",
                    "Aling salita ang nagsisimula sa KL?|KLASE~KAMA~SAMA|KLASE",
                    "Anong unang dalawang katinig sa BRASO?|BR~BA~BO|BR"
                }));

            sessions.Add(CreatePracticeSession(
                30,
                "Pagsasanay (Titik D, H, at Klaster)",
                "Pagkilala sa D, H, at mga kambal katinig",
                "Basahin ang bawat salita at piliin ang tamang simula.",
                new[]
                {
                    "Anong titik ang simula ng DAHON?|D~H~B|D",
                    "Anong titik ang simula ng HIPON?|H~D~P|H",
                    "Alin ang salitang may klaster sa unahan?|PLATO~PUNO~DILA|PLATO",
                    "Alin ang nagsisimula sa TR?|TRAPO~TASA~ROSAS|TRAPO",
                    "Aling salita ang walang klaster sa unahan?|HALAMAN~KLASE~PRUTAS|HALAMAN"
                }));

            sessions.Add(CreateCheckInSession(31, 3));

            sessions.Add(CreateLetterLesson(
                32,
                "Titik Cc at Ff",
                "Kilalanin at isulat ang Cc at Ff sa mga hiram na salita.",
                new[] { "C", "F" },
                new[] { "casa", "cero", "fiesta", "foto", "cafe" },
                new[]
                {
                    "☕ CAFE|true",
                    "🏠 CASA|true",
                    "🎉 FIESTA|true",
                    "📷 FOTO|true",
                    "🐟 ISDA|false"
                },
                new[]
                {
                    "☕ CAFE|C",
                    "🏠 CASA|C",
                    "🎉 FIESTA|F",
                    "📷 FOTO|F"
                },
                "C,F,S"));

            sessions.Add(CreateLetterLesson(
                33,
                "Titik Ññ, Jj, at Qq",
                "Kilalanin at isulat ang Ññ, Jj, at Qq.",
                new[] { "Ñ", "J", "Q" },
                new[] { "jugo", "jabón", "queso", "quince", "casa" },
                new[]
                {
                    "🪅 PIÑATA|false",
                    "🧔 SEÑOR|false",
                    "🧃 JUGO|true",
                    "🧀 QUESO|true",
                    "🍇 UBAS|false"
                },
                new[]
                {
                    "🧃 JUGO|J",
                    "🧀 QUESO|Q",
                    "🪅 PIÑATA|P",
                    "🧔 SEÑOR|S"
                },
                "Ñ,J,Q,P,S",
                new[] { "J", "Q" },
                new[]
                {
                    "Aling salita ang may Ñ?|PIÑATA~JUGO~QUESO|PIÑATA",
                    "Aling salita ang nagsisimula sa Q?|QUESO~SEÑOR~JUGO|QUESO"
                }));

            sessions.Add(CreateLetterLesson(
                34,
                "Titik Vv, Xx, at Zz",
                "Kilalanin at isulat ang Vv, Xx, at Zz.",
                new[] { "V", "X", "Z" },
                new[] { "vaso", "vinta", "xilópono", "taxi", "zebra" },
                new[]
                {
                    "🥛 VASO|true",
                    "⛵ VINTA|true",
                    "🩻 X-RAY|true",
                    "🦓 ZEBRA|true",
                    "⚽ BOLA|false"
                },
                new[]
                {
                    "🥛 VASO|V",
                    "🩻 X-RAY|X",
                    "🦓 ZEBRA|Z",
                    "⚽ BOLA|B"
                },
                "V,X,Z,B"));

            sessions.Add(CreatePracticeSession(
                35,
                "Pagsasanay (Titik C, F, Ñ, J, Q, V, X, at Z)",
                "Pagkilala sa mga titik C, F, Ñ, J, Q, V, X, at Z",
                "Piliin ang titik na makikita sa simula o loob ng salita.",
                new[]
                {
                    "Anong titik ang simula ng CASA?|C~F~Q|C",
                    "Anong titik ang simula ng FIESTA?|F~V~Z|F",
                    "Aling salita ang may Ñ?|SEÑOR~VASO~QUESO|SEÑOR",
                    "Anong titik ang simula ng JUGO?|J~X~C|J",
                    "Anong titik ang simula ng QUESO?|Q~Z~F|Q",
                    "Anong titik ang simula ng VASO?|V~X~Ñ|V",
                    "Aling salita ang nagsisimula sa X?|X-RAY~ZEBRA~CASA|X-RAY",
                    "Anong titik ang simula ng ZEBRA?|Z~J~V|Z"
                }));

            sessions.Add(CreateCheckInSession(36, 4));

            ValidateCurriculum(sessions);

            return sessions;
        }

        private static void ValidateCurriculum(
            IReadOnlyList<LessonSession> sessions)
        {
            if (sessions.Count != 36)
            {
                throw new InvalidOperationException(
                    $"The curriculum must contain 36 sessions; found {sessions.Count}.");
            }

            for (int index = 0; index < sessions.Count; index++)
            {
                LessonSession session = sessions[index];
                int expectedNumber = index + 1;

                if (session.SessionNumber != expectedNumber)
                {
                    throw new InvalidOperationException(
                        $"Expected session {expectedNumber}, " +
                        $"but found session {session.SessionNumber}.");
                }

                if (session.IsCheckIn !=
                    session.CheckInNumber.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Session {session.SessionNumber} has invalid check-in metadata.");
                }

                if (!session.IsCheckIn &&
                    session.Steps.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Session {session.SessionNumber} has no activities.");
                }
            }
        }

        private static LessonSession CreatePracticeSession(
            int sessionNumber,
            string title,
            string description,
            string introduction,
            string[] questions)
        {
            ValidateWordChoiceItems(
                sessionNumber,
                questions);

            List<LessonStep> steps =
                new List<LessonStep>
                {
                    new LessonStep
                    {
                        StepNumber = 1,
                        SectionName = "PAMBUNGAD",
                        Title = title,
                        Instruction = description,
                        Prompt = introduction,
                        VoiceoverText = introduction,
                        StepType = LessonStepType.Introduction
                    },
                    new LessonStep
                    {
                        StepNumber = 2,
                        SectionName = "GAWAIN",
                        Title = "Piliin ang tamang sagot",
                        Instruction =
                            "Basahin o pakinggan ang bawat tanong. " +
                            "Pindutin ang sagot na sa tingin mo ay tama.",
                        Prompt = introduction,
                        VoiceoverText = introduction,
                        StepType = LessonStepType.WordChoiceActivity,
                        Items = questions.ToList()
                    },
                    CreateReviewStep(
                        3,
                        title)
                };

            return new LessonSession
            {
                SessionNumber = sessionNumber,
                Title = title,
                ShortTitle = title,
                Description = description,
                IsCheckIn = false,
                Steps = steps
            };
        }

        private static LessonSession CreateLetterLesson(
            int sessionNumber,
            string title,
            string description,
            string[] letters,
            string[] caseWords,
            string[] soundItems,
            string[] beginningLetterItems,
            string beginningLetterOptions,
            string[]? caseTargets = null,
            string[]? practiceQuestions = null)
        {
            List<LessonStep> steps =
                new List<LessonStep>
                {
                    new LessonStep
                    {
                        StepNumber = 1,
                        SectionName = "PAMBUNGAD",
                        Title = title,
                        Instruction = description,
                        Prompt =
                            "Pakinggan, bigkasin, at kilalanin " +
                            "ang mga titik sa sesyong ito.",
                        VoiceoverText =
                            "Kilalanin natin ang mga titik sa sesyong ito.",
                        StepType = LessonStepType.Introduction
                    }
                };

            foreach (string letter in letters)
            {
                string lowercase =
                    letter.ToLowerInvariant();

                steps.Add(
                    new LessonStep
                    {
                        StepNumber = steps.Count + 1,
                        SectionName = "MAGSANAY MAGSULAT",
                        Title = $"Isulat ang {letter} at {lowercase}",
                        Instruction =
                            $"Gamitin ang daliri upang isulat ang " +
                            $"malaking {letter} at maliit na {lowercase}.",
                        Prompt =
                            $"{letter}   {letter}   {letter}      " +
                            $"{lowercase}   {lowercase}   {lowercase}",
                        VoiceoverText =
                            $"Magsanay sa pagsulat ng {letter} at {lowercase}.",
                        StepType = LessonStepType.LetterWriting,
                        Items = new List<string>
                        {
                            letter,
                            lowercase
                        }
                    });
            }

            string[] targets =
                caseTargets ??
                letters.Where(letter => letter.Length == 1).ToArray();

            if (caseWords.Length > 0 && targets.Length > 0)
            {
                steps.Add(
                    new LessonStep
                    {
                        StepNumber = steps.Count + 1,
                        SectionName = "GAWAIN",
                        Title = "Hanapin ang mga titik",
                        Instruction =
                            "Pindutin ang bawat target na titik sa mga salita.",
                        Prompt =
                            $"Hanapin ang {string.Join(", ", targets)} " +
                            "sa bawat salita.",
                        Placeholder = string.Join(",", targets),
                        VoiceoverText =
                            $"Hanapin ang mga titik na {string.Join(", ", targets)}.",
                        StepType = LessonStepType.LetterCaseActivity,
                        Items = caseWords.ToList()
                    });
            }

            if (soundItems.Length > 0)
            {
                steps.Add(
                    new LessonStep
                    {
                        StepNumber = steps.Count + 1,
                        SectionName = "GAWAIN",
                        Title = "Pakinggan ang unang tunog",
                        Instruction =
                            "Piliin ang mga salitang nagsisimula sa " +
                            "target na tunog o titik.",
                        Prompt =
                            "Pindutin ang mga salitang nagsisimula sa " +
                            "mga titik na pinag-aaralan.",
                        VoiceoverText =
                            "Pakinggan ang unang tunog at piliin ang katugmang salita.",
                        StepType = LessonStepType.BeginningSoundActivity,
                        Items = soundItems.ToList()
                    });
            }

            if (beginningLetterItems.Length > 0)
            {
                ValidateBeginningLetterItems(
                    sessionNumber,
                    beginningLetterItems,
                    beginningLetterOptions);

                steps.Add(
                    new LessonStep
                    {
                        StepNumber = steps.Count + 1,
                        SectionName = "GAWAIN",
                        Title = "Piliin ang simulang titik",
                        Instruction =
                            "Basahin ang salita at piliin ang titik " +
                            "na maririnig sa unahan nito.",
                        Prompt =
                            $"Pagpipilian: {beginningLetterOptions}.",
                        Placeholder = beginningLetterOptions,
                        VoiceoverText =
                            "Piliin ang titik na maririnig sa unahan ng salita.",
                        StepType = LessonStepType.BeginningLetterActivity,
                        Items = beginningLetterItems.ToList()
                    });
            }

            if (practiceQuestions != null &&
                practiceQuestions.Length > 0)
            {
                ValidateWordChoiceItems(
                    sessionNumber,
                    practiceQuestions);

                steps.Add(
                    new LessonStep
                    {
                        StepNumber = steps.Count + 1,
                        SectionName = "DAGDAG NA PAGSASANAY",
                        Title = "Subukan pa",
                        Instruction =
                            "Piliin ang sagot na tumutugma sa tanong.",
                        Prompt =
                            "Pag-isipan ang bawat pagpipilian bago sumagot.",
                        VoiceoverText =
                            "Piliin ang sagot na tumutugma sa tanong.",
                        StepType = LessonStepType.WordChoiceActivity,
                        Items = practiceQuestions.ToList()
                    });
            }

            steps.Add(
                CreateReviewStep(
                    steps.Count + 1,
                    title));

            return new LessonSession
            {
                SessionNumber = sessionNumber,
                Title = title,
                ShortTitle = title,
                Description = description,
                IsCheckIn = false,
                Steps = steps
            };
        }

        private static LessonStep CreateReviewStep(
            int stepNumber,
            string title)
        {
            return new LessonStep
            {
                StepNumber = stepNumber,
                SectionName = "PAGTATAPOS",
                Title = $"Balikan ang {title}",
                Instruction =
                    "Tingnan ang resulta ng iyong mga sagot. " +
                    "Maaari mong balikan ang mga gawain bago matapos.",
                Prompt =
                    "Magaling sa pagsasanay! Ipagpatuloy ang pagbabasa.",
                VoiceoverText =
                    "Balikan natin ang mga natutuhan mo.",
                StepType = LessonStepType.Review
            };
        }

        private static void ValidateWordChoiceItems(
            int sessionNumber,
            IEnumerable<string> questions)
        {
            foreach (string question in questions)
            {
                string[] parts = question.Split('|');
                if (parts.Length != 3)
                {
                    throw new InvalidOperationException(
                        $"Invalid word-choice question in session {sessionNumber}.");
                }

                string[] options = parts[1]
                    .Split('~', StringSplitOptions.RemoveEmptyEntries)
                    .Select(option => option.Trim())
                    .ToArray();

                if (string.IsNullOrWhiteSpace(parts[0]) ||
                    options.Length < 2 ||
                    !options.Contains(
                        parts[2].Trim(),
                        StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Invalid answer or options in session {sessionNumber}: " +
                        $"{parts[0]}.");
                }
            }
        }

        private static void ValidateBeginningLetterItems(
            int sessionNumber,
            IEnumerable<string> items,
            string optionsText)
        {
            string[] options = optionsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(option => option.Trim())
                .ToArray();

            foreach (string item in items)
            {
                string[] parts = item.Split('|');
                if (parts.Length != 2 ||
                    !options.Contains(
                        parts[1].Trim(),
                        StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Invalid beginning-letter answer in session {sessionNumber}: " +
                        $"{item}.");
                }
            }
        }

        private static LessonSession CreateCheckInSession(
            int sessionNumber,
            int checkInNumber)
        {
            return new LessonSession
            {
                SessionNumber = sessionNumber,

                Title =
                    $"Check-in #{checkInNumber}",

                ShortTitle =
                    $"Check-in #{checkInNumber}",

                Description =
                    $"Progress Check-in #{checkInNumber}",

                IsCheckIn = true,
                CheckInNumber = checkInNumber,

                Steps =
                    new List<LessonStep>()
            };
        }
    }
}