import entity.BokEntity;
import jakarta.persistence.EntityManager;
import jakarta.persistence.EntityManagerFactory;
import jakarta.persistence.Persistence;
import jakarta.persistence.Query;

import java.util.List;
import java.util.Scanner;

public class Main {
    private static Scanner scanner = new Scanner(System.in);

    public static void main(String[] args) {

        boolean quit = false;
        printActions();
        while(!quit) {
            System.out.println("\nVälj (8 för att visa val):");
            int action = scanner.nextInt();
            scanner.nextLine();

            switch (action) {
                case 0:
                    System.out.println("\nStänger ner...");
                    quit = true;
                    break;

                case 1:
                    showBook();
                    break;
                case 2:
                    showBookByAuthor();
                    break;
                case 3:
                    countBooksQuery();
                    break;
                case 4:
                    newBookInput();
                    break;
                case 5:
                    updateBook();
                    break;
                case 6:
                    deleteBook();
                    break;
                case 7:
                    showBooksQuery();
                    //showBooksNamedQuery();
                    break;
                case 8:
                    printActions();
                    break;
            }
        }
    }

    private static void printActions() {
        System.out.println("\nVälj:\n");
        System.out.println("0  - Stäng av\n" +
                "1  - Visa en bok\n" +
                "2  - Visa alla böcker för en författare\n" +
                "3  - Visa antal böcker\n" +
                "4  - Lägg till en bok\n" +
                "5  - Uppdatera en bok\n" +
                "6  - Ta bort en bok\n" +
                "7  - Visa alla böcker.\n" +
                "8  - Visa en lista över alla val.");
    }

    private static void newBook(String titel, String forfattare){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();
        entityManager.getTransaction().begin();
        BokEntity bok = new BokEntity();

        bok.setBokTitel(titel);
        bok.setBokForfattare(forfattare);


        entityManager.persist(bok);
        entityManager.getTransaction().commit();
        entityManager.close();
        entityManagerFactory.close();

        System.out.println("Du har lagt till en bok");
    }

    private static void newBookInput(){
        System.out.println("Skriv in titel på boken: ");
        String inputTitel = scanner.nextLine();
        System.out.println("Skriv in författare på boken: ");
        String inputForfattare = scanner.nextLine();
        newBook(inputTitel,inputForfattare);
        scanner.nextLine();
    }

    private static void showBook(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();
        BokEntity bok = entityManager.find( BokEntity.class, 1 );
        System.out.println("Bok ID = " + bok.getBokId( ));
        System.out.println("Bok Titel = " + bok.getBokTitel( ));
    }

    private static void updateBook(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();
        entityManager.getTransaction().begin();
        BokEntity bok = entityManager.find( BokEntity.class, 1 );
        bok.setBokTitel("Vi på Saltkråkan");
        bok.setBokForfattare("Astrid Lindgren");
        entityManager.persist(bok);
        entityManager.getTransaction().commit();
        entityManager.close();
        entityManagerFactory.close();

        System.out.println("Du har uppdaterat boken");

    }

    private static void deleteBook(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();
        entityManager.getTransaction().begin();
        BokEntity bok = entityManager.find( BokEntity.class, 3 );

        entityManager.remove( bok );
        entityManager.getTransaction().commit();
        entityManager.close();
        entityManagerFactory.close();

        System.out.println("Du har tagit bort boken");
    }

    private static void showBookByAuthor(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();

        Query query = entityManager.createNamedQuery("bokForfattareQuery");

        query.setParameter("forfattare", "Astrid Lindgren");
        List<BokEntity> list = query.getResultList( );

        for( BokEntity b:list ){
            System.out.print("Bok ID :" + b.getBokTitel());
            System.out.println("\t Bok Titel :" + b.getBokForfattare());
        }
    }

    private static void showBooksNamedQuery(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();

        Query query = entityManager.createNamedQuery("bokQuery");

        List<BokEntity> list = query.getResultList( );

        for( BokEntity b:list ){
            System.out.print("Bok ID :" + b.getBokTitel());
            System.out.println("\t Bok Titel :" + b.getBokForfattare());
        }
    }

    private static void showBooksQuery(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();

        Query query = entityManager.createQuery("SELECT b FROM BokEntity b");

        List<BokEntity> list = query.getResultList( );

        for( BokEntity b:list ){
            System.out.print("Bok ID :" + b.getBokTitel());
            System.out.println("\t Bok Titel :" + b.getBokForfattare());
        }
    }

    private static void countBooksQuery(){
        EntityManagerFactory entityManagerFactory = Persistence.createEntityManagerFactory("default");
        EntityManager entityManager = entityManagerFactory.createEntityManager();

        Query query = entityManager.createQuery("SELECT COUNT(b.bokId) FROM BokEntity b");

        System.out.println(query.getSingleResult());
    }

}

